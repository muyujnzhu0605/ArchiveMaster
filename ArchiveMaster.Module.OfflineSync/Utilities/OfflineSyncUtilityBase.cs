using FzLib;
using ArchiveMaster.Model;
using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace ArchiveMaster.Utility
{
    public class OfflineSyncUtilityBase
    {

        /// <summary>
        /// 已经收到停止导出信号
        /// </summary>
        protected bool stopping = false;
        public event EventHandler<MessageEventArgs> MessageReceived;

        public event EventHandler<ProgressUpdatedEventArgs> ProgressUpdated;

        /// <summary>
        /// 停止导出（打包）
        /// </summary>
        public void Stop()
        {
            stopping = true;
        }
        /// <summary>
        /// 复制并获取MD5
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        protected string CopyAndGetMd5(string from, string to)
        {
            int bufferSize = 1024 * 1024; //1MB的缓冲区
            using MD5 md5 = MD5.Create();
            using FileStream fileStream = new FileStream(to, FileMode.Create, FileAccess.Write, FileShare.None);
            using FileStream fs = new FileStream(from, FileMode.Open, FileAccess.Read);
            try
            {
                fileStream.SetLength(fs.Length);
                int bytesRead = -1;
                byte[] bytes = new byte[bufferSize];
                int offset = 0;
                while ((bytesRead = fs.Read(bytes, 0, bufferSize)) > 0)
                {
                    md5.TransformBlock(bytes, 0, bytesRead, null, 0);
                    fileStream.Write(bytes, 0, bytesRead);
                    offset += bytesRead;
                }
                md5.TransformFinalBlock(new byte[0], 0, 0);
                fs.Close();
                fs.Dispose();
                fileStream.Close();
                fileStream.Dispose();
                File.SetLastWriteTime(to, File.GetLastWriteTime(from));
                return BitConverter.ToString(md5.Hash).Replace("-", "");
            }
            catch (Exception ex)
            {
                fs.Close();
                fs.Dispose();
                fileStream.Close();
                fileStream.Dispose();
                if (File.Exists(to))
                {
                    try
                    {
                        File.Delete(to);
                    }
                    catch
                    {

                    }
                }
                throw;
            }
        }

        protected string GetMD5(string file)
        {
            using MD5 md5 = MD5.Create();
            using var stream = File.OpenRead(file);
            md5.ComputeHash(stream);
            return BitConverter.ToString(md5.Hash).Replace("-", "");
        }

        protected void InvokeMessageReceivedEvent(string message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(message));
        }
        protected void InvokeProgressReceivedEvent(long current, long total)
        {
            ProgressUpdated?.Invoke(this, new ProgressUpdatedEventArgs(current, total));
        }


        protected static void WriteToZip(object obj, string zipPath)
        {
            var json = JsonSerializer.Serialize(obj);
            byte[] bytes = new UTF8Encoding(true).GetBytes(json);
            using FileStream fs = new FileStream(zipPath, FileMode.Create);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create);
            using Stream es = zip.CreateEntry("DATA").Open();
            es.Write(bytes, 0, bytes.Length);
        }

        protected static T ReadFromZip<T>(string zipPath)
        {
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException();
            }
            using FileStream fs = new FileStream(zipPath, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);
            TextReader reader = new StreamReader(zip.Entries[0].Open(),new UTF8Encoding(true));
            string json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}


