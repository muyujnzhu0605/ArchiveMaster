using System;
using System.Numerics;

namespace ArchiveMaster.Services
{
    public class ProgressUpdateEventArgs : EventArgs 
    {
        public double Progress { get; }

        public ProgressUpdateEventArgs(double progress)
        {
            if (!double.IsNaN(progress) && progress is < 0 or > 1)
            {
                throw new ArgumentException("百分比应在0和1之间，或用NaN表示不确定", nameof(progress));
            }
            Progress = progress;
        }
    }
}