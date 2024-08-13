namespace ArchiveMaster.Utilities;

public static class CancellationExtension
{
    public static IEnumerable<T> WithCancellationToken<T>(this IEnumerable<T> source,
        CancellationToken cancellationToken)
    {
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}