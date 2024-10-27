namespace ArchiveMaster.Helpers;

public static class OptionsHelper
{
    public static EnumerationOptions GetEnumerationOptions(bool includingSubDirs = true)
    {
        return new EnumerationOptions()
        {
            IgnoreInaccessible = true,
            AttributesToSkip = 0,
            RecurseSubdirectories = includingSubDirs,
        };
    }
}