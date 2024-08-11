namespace ArchiveMaster.Utilities;

public class MessageUpdateEventArgs(string message)
{
    public string Message { get; } = message;
}