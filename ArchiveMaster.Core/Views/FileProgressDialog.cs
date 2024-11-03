using System.Diagnostics;

namespace ArchiveMaster.Views;

public class FileProgressDialog : ProgressDialog
{
    private CancellationTokenSource cts;

    public FileProgressDialog()
    {
        Title = "正在导出";
        SecondaryButtonContent = "中断";
        Width = 600;
    }

    public async Task CopyFileAsync(string sourcePath, string destinationPath, DateTime time,
        int bufferSize = 128 * 1024)
    {
        Message = "正在复制" + Path.GetFileName(destinationPath);
        cts = new CancellationTokenSource();
        var buffer = new byte[bufferSize];

        Maximum = new FileInfo(sourcePath).Length;
        try
        {
            await CopyFileAsync(sourcePath, destinationPath, time, buffer);
            OnCopyComplete();
        }
        catch (OperationCanceledException)
        {
            OnCopyCanceled(destinationPath);
        }
        catch (Exception ex)
        {
            OnCopyError(ex);
        }
    }

    public async Task CopyFilesAsync(IList<string> sourcePaths, IList<string> destinationPaths, IList<DateTime> times,
        int bufferSize = 128 * 1024)
    {
        if (sourcePaths.Count != destinationPaths.Count || sourcePaths.Count != times.Count)
        {
            throw new ArgumentException("源、目标或时间的数量不同");
        }

        int i = 0;
        try
        {
            cts = new CancellationTokenSource();
            var buffer = new byte[bufferSize];

            Maximum = sourcePaths.Select(p => new FileInfo(p).Length).Sum();

            for (; i < sourcePaths.Count; i++)
            {
                Message = "正在复制" + Path.GetFileName(destinationPaths[i]);
                await CopyFileAsync(sourcePaths[i], destinationPaths[i], times[i], buffer);
            }

            OnCopyComplete();
        }
        catch (OperationCanceledException)
        {
            OnCopyCanceled(destinationPaths[i]);
        }
        catch (Exception ex)
        {
            OnCopyError(ex);
        }
    }

    protected override void OnCloseButtonClick()
    {
        Close();
    }

    protected override void OnSecondaryButtonClick()
    {
        SecondaryButtonContent = null;
        cts?.Cancel();
    }

    private async Task CopyFileAsync(string sourcePath, string destinationPath, DateTime time, byte[] buffer)
    {
        var destDir = new FileInfo(destinationPath).Directory;
        if (!destDir.Exists)
        {
            destDir.Create();
        }

        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        await using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
        {
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                cts.Token.ThrowIfCancellationRequested();
                await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cts.Token);
                Value += bytesRead;
#if DEBUG
                await Task.Delay(100);
#endif
            }
        }

        File.SetLastWriteTime(destinationPath, time);
    }

    private void OnCopyCanceled(string destinationPath)
    {
        try
        {
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
        }
        catch
        {
        }

        Close();
    }

    private void OnCopyComplete()
    {
        CloseButtonContent = "完成";
        SecondaryButtonContent = null;
        Message = "完成";
    }

    private void OnCopyError(Exception ex)
    {
        Title = "错误";
        Message = ex.Message;
        CloseButtonContent = "取消";
        SecondaryButtonContent = null;
    }
}