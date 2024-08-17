using System;

namespace ArchiveMaster.Platforms
{
    public interface IBackCommandService
    {
        public void RegisterBackCommand(Func<bool> backAction);
    }
}
