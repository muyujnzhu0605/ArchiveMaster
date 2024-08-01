using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using ArchiveMaster.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;

namespace ArchiveMaster.ViewModels;

[DebuggerDisplay("{Name}")]
public partial class FileInfoWithStatus : SimpleFileInfo
{
    [property: JsonIgnore]
    [ObservableProperty]
    private bool complete;

    [property: JsonIgnore]
    [ObservableProperty]
    private bool isChecked = true;

    [property: JsonIgnore]
    [ObservableProperty]
    private string message;

}
