using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveMaster.ViewModels;

public partial class EncryptorViewModel : TwoStepViewModelBase<EncryptorUtility>
{
    [ObservableProperty]
    private bool isEncrypting = true;

    [ObservableProperty]
    private List<EncryptorFileInfo> processingFiles;

    public CipherMode[] CipherModes => Enum.GetValues<CipherMode>();

    public PaddingMode[] PaddingModes => Enum.GetValues<PaddingMode>();

    public EncryptorViewModel()
    {
        AppConfig.Instance.BeforeSaving += (s, e) =>
        {
            if (!Config.RememberPassword)
            {
                Config.Password = null;
            }
        };
    }

    public override EncryptorConfig Config { get; } = AppConfig.Instance.Get<EncryptorConfig>();

    protected override async Task OnExecutedAsync(CancellationToken token)
    {
        if (ProcessingFiles.Any(p => p.Error != null))
        {
            string typeDesc = IsEncrypting ? "加密" : "解密";
            var errorDetails = ProcessingFiles.Where(p => p.Error != null).Select(p => $"{p.Name}：{p.Message}");
            await WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = $"{typeDesc}存在错误",
                Message = $"{typeDesc}过程已结束，部分文件{typeDesc}失败，请检查",
                Detail = string.Join(Environment.NewLine, errorDetails)
            }).Task;
        }
    }

    [RelayCommand]
    private async Task CopyErrorAsync(Exception exception)
    {
        await WeakReferenceMessenger.Default.Send(new GetClipboardMessage())
            .Clipboard
            .SetTextAsync(exception.ToString());
    }

    protected override Task OnInitializingAsync()
    {
        Config.Type = IsEncrypting
            ? EncryptorConfig.EncryptorTaskType.Encrypt
            : EncryptorConfig.EncryptorTaskType.Decrypt;
        return base.OnInitializingAsync();
    }


    protected override Task OnInitializedAsync()
    {
        ProcessingFiles = Utility.ProcessingFiles;
        return base.OnInitializedAsync();
    }

    protected override void OnReset()
    {
        ProcessingFiles = null;
    }
}