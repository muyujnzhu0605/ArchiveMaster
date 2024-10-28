namespace ArchiveMaster.ViewModels.FileSystem
{
    public class DiscFilePackageCollection
    {
        public List<FileSystem.DiscFilePackage> DiscFilePackages { get; } = new List<FileSystem.DiscFilePackage>();
        public List<FileSystem.DiscFile> SizeOutOfRangeFiles { get; } = new List<FileSystem.DiscFile>();
    }
}
