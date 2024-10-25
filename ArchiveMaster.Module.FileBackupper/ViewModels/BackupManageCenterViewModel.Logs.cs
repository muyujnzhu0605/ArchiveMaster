using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Converters;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels;

public partial class BackupManageCenterViewModel
{
    [ObservableProperty]
    private ObservableCollection<BackupLogEntity> logs;

    [RelayCommand]
    private Task ShowDetailAsync(BackupLogEntity log)
    {
        return this.SendMessage(new CommonDialogMessage()
        {
            Type = CommonDialogMessage.CommonDialogType.Ok,
            Message = log.Message,
            Title = LogLevelConverter.GetDescription(log.Type),
            Detail = log.Detail
        }).Task;
    }
}