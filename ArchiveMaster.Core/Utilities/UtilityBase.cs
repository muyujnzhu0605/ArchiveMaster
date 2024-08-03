namespace ArchiveMaster.Utilities
{
    public abstract class UtilityBase
    {
        public event EventHandler<ProgressUpdateEventArgs<int>> ProgressUpdate;

        protected void NotifyProgressUpdate(string message)
        {
            NotifyProgressUpdate(-1, 0, message);
        }

        protected void NotifyProgressUpdate(int maximum, int current, string message)
        {
            ProgressUpdate?.Invoke(this, new ProgressUpdateEventArgs<int>(maximum, current, message));
        }

        protected void NotifyProgressUpdate(long maximum, long current, string message)
        {
            ProgressUpdate?.Invoke(this,
                new ProgressUpdateEventArgs<int>(10000, (int)(10000.0 / maximum * current), message));
        }
    }
}