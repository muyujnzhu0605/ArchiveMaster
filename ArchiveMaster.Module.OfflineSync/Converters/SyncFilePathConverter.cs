using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncFileInfo = ArchiveMaster.ViewModels.FileSystem.SyncFileInfo;

namespace ArchiveMaster.Converters
{
    public class SyncFilePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if(value is not SyncFileInfo file)
            {
                throw new ArgumentException($"{nameof(value)}必须为{nameof(SyncFileInfo)}类型");
            }

            string topDirName = Path.GetFileName(file.TopDirectory);
            string relativePath = file.RelativePath;
            switch (file.UpdateType)
            {
                case FileUpdateType.None:
                case FileUpdateType.Add:
                case FileUpdateType.Modify:
                case FileUpdateType.Delete:
                    return Path.Combine(topDirName,relativePath);
                case FileUpdateType.Move:
                    return $"{Path.Combine(topDirName, file.OldRelativePath)} -> {Path.Combine(topDirName, relativePath)}";
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
