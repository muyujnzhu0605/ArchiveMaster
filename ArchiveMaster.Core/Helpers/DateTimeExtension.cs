namespace ArchiveMaster.Helpers;

public static class DateTimeExtension
{
    public static DateTime TruncateToSecond(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            return dateTime;
        }

        return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
    }

}