using System.Diagnostics;
using ArchiveMaster.ViewModels;
using Avalonia.Controls;

namespace ArchiveMaster.Views
{
    public partial class BackupTaskDataGrid : DataGrid
    {
        protected override Type StyleKeyOverride => typeof(DataGrid);

        public BackupTaskDataGrid()
        {
            InitializeComponent();
        }
    }
}