using System;
using System.Numerics;

namespace ArchiveMaster.Utilities
{
    public class ProgressUpdateEventArgs<T>(T maximum, T current, string message) : EventArgs where T : INumber<T>
    {
        public T Maximum { get; } = maximum;
        public T Current { get; } = current;
        public string Message { get; } = message;
    }
}
