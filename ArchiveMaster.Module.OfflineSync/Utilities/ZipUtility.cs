using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ArchiveMaster.Utilities
{
    public static class ZipUtility
    {
        public static void WriteToZip(object obj, string zipPath)
        {
            var json = JsonSerializer.Serialize(obj);
            byte[] bytes = new UTF8Encoding(true).GetBytes(json);
            using FileStream fs = new FileStream(zipPath, FileMode.Create);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create);
            using Stream es = zip.CreateEntry("DATA").Open();
            es.Write(bytes, 0, bytes.Length);
        }

        public static T ReadFromZip<T>(string zipPath)
        {
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException();
            }

            using FileStream fs = new FileStream(zipPath, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);
            TextReader reader = new StreamReader(zip.Entries[0].Open(), new UTF8Encoding(true));
            string json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}