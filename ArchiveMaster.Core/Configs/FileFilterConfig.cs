using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class FileFilterConfig : ObservableObject
    {
        [ObservableProperty]
        private string excludeFiles = "";

        [ObservableProperty]
        private string excludeFolders = "";

        [ObservableProperty]
        private string excludePaths = "";

        [ObservableProperty]
        private string includeFiles = "*";

        [ObservableProperty]
        private string includeFolders = "*";
        
        [ObservableProperty]
        private string includePaths = "*";

        [ObservableProperty]
        private bool useRegex;

        partial void OnUseRegexChanged(bool value)
        {
            if (value)
            {
                IncludeFiles = IncludeFiles == "*" ? ".*" : IncludeFiles;
                IncludeFolders = IncludeFolders == "*" ? ".*" : IncludeFolders;
                IncludePaths = IncludePaths == "*" ? ".*" : IncludePaths;
            }
            else
            {
                IncludeFiles = IncludeFiles == ".*" ? "*" : IncludeFiles;
                IncludeFolders = IncludeFolders == ".*" ? "*" : IncludeFolders;
                IncludePaths = IncludePaths == ".*" ? "*" : IncludePaths;
            }
        }
    }
}