using ArchiveMaster.Model;
using ArchiveMaster.Views;
using ArchiveMaster.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public static class PanelHelper
    {
        public static void RegisterMessageAndProgressEvent<T>(OfflineSyncUtilityBase utility, OfflineSyncViewModelBase<T> viewModel) where T : FileInfoWithStatus
        {
            utility.MessageReceived += (s, e) =>
            {
                viewModel.Message = e.Message;
            };
            utility.ProgressUpdated += (s, e) =>
            {
                if (e.MaxValue != viewModel.ProgressMax)
                {
                    viewModel.ProgressMax = e.MaxValue;
                }
                viewModel.Progress = e.Value;
            };
        }
    }
}
