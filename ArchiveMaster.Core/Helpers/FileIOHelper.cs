using System.Buffers;
using System.Security.Cryptography;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ArchiveMaster.Helpers
{
    public static class FileIOHelper
    {
        private static int GetOptimalBufferSize(long fileLength)
        {
            return fileLength switch
            {
                < 1 * 1024 * 1024 => 16 * 1024,       // 小文件（<1MB）：16KB
                < 32 * 1024 * 1024 => 1 * 1024 * 1024, // 中等文件（1MB~32MB）：1MB
                _ => 4 * 1024 * 1024                   // 大文件（>32MB）：4MB
            };
        }

        /// <summary>
        /// 高性能文件复制（双缓冲流水线）
        /// </summary>
        public static async Task CopyFileAsync(
            string sourceFilePath,
            string destinationFilePath,
            CancellationToken cancellationToken = default,
            int bufferSize = 0)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("源文件不存在", sourceFilePath);

            // 确保目标目录存在
            string directory = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 动态调整缓冲区
            if (bufferSize <= 0)
            {
                var fileInfo = new FileInfo(sourceFilePath);
                bufferSize = GetOptimalBufferSize(fileInfo.Length);
            }

            // 创建双缓冲通道（容量=2）
            var bufferChannel = Channel.CreateBounded<(byte[] buffer, int bytesRead)>(
                new BoundedChannelOptions(2)
                {
                    SingleWriter = true,
                    SingleReader = true,
                    FullMode = BoundedChannelFullMode.Wait
                });

            try
            {
                await using var sourceStream = new FileStream(
                    sourceFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                await using var destinationStream = new FileStream(
                    destinationFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize,
                    FileOptions.Asynchronous | FileOptions.WriteThrough);

                // 启动并行任务
                var readTask = ReadDataAsync(sourceStream, bufferChannel.Writer, bufferSize, cancellationToken);
                var writeTask = WriteDataAsync(destinationStream, bufferChannel.Reader, cancellationToken);

                await Task.WhenAll(readTask, writeTask);

                // 复制文件属性
                var sourceInfo = new FileInfo(sourceFilePath);
                File.SetLastWriteTimeUtc(destinationFilePath, sourceInfo.LastWriteTimeUtc);
                File.SetCreationTimeUtc(destinationFilePath, sourceInfo.CreationTimeUtc);
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(destinationFilePath))
                    File.Delete(destinationFilePath);
                throw;
            }
        }

        private static async Task ReadDataAsync(
            FileStream sourceStream,
            ChannelWriter<(byte[] buffer, int bytesRead)> writer,
            int bufferSize,
            CancellationToken ct)
        {
            try
            {
                byte[] readBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                try
                {
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(readBuffer.AsMemory(0, bufferSize), ct)) > 0)
                    {
                        await writer.WriteAsync((readBuffer, bytesRead), ct);
                        readBuffer = ArrayPool<byte>.Shared.Rent(bufferSize); // 获取新缓冲区
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(readBuffer);
                }
            }
            finally
            {
                writer.Complete();
            }
        }

        private static async Task WriteDataAsync(
            FileStream destinationStream,
            ChannelReader<(byte[] buffer, int bytesRead)> reader,
            CancellationToken ct)
        {
            await foreach (var (buffer, bytesRead) in reader.ReadAllAsync(ct))
            {
                await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// 高性能SHA1计算（双缓冲流水线）
        /// </summary>
        public static async Task<string> ComputeSha1Async(
            string filePath,
            CancellationToken cancellationToken = default,
            int bufferSize = 0)
        {
            if (bufferSize <= 0)
            {
                var fileInfo = new FileInfo(filePath);
                bufferSize = GetOptimalBufferSize(fileInfo.Length);
            }

            using var sha1 = SHA1.Create();
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var bufferChannel = Channel.CreateBounded<(byte[] buffer, int bytesRead)>(2);

            var readTask = ReadDataAsync(stream, bufferChannel.Writer, bufferSize, cancellationToken);
            var computeTask = ComputeHashAsync(sha1, bufferChannel.Reader, cancellationToken);

            await Task.WhenAll(readTask, computeTask);
            return Convert.ToHexString(sha1.Hash!);
        }

        private static async Task ComputeHashAsync(
            SHA1 sha1,
            ChannelReader<(byte[] buffer, int bytesRead)> reader,
            CancellationToken ct)
        {
            await foreach (var (buffer, bytesRead) in reader.ReadAllAsync(ct))
            {
                sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                ArrayPool<byte>.Shared.Return(buffer);
            }
            sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        }
    }
}