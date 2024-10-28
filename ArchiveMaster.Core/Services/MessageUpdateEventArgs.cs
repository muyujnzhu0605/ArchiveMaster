namespace ArchiveMaster.Services;

public class MessageUpdateEventArgs(string message)
{
    public string Message { get; } = message;
}