using System.Collections;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels;

public partial class PackingViewModel(PackingConfig config) : TwoStepViewModelBase<PackingUtility,PackingConfig>(config)
{
    [ObservableProperty]
    private List<DiscFilePackage> discFilePackages;

    [ObservableProperty]
    private DateTime earliestDateTime = new DateTime(1, 1, 1);

    [ObservableProperty]
    private DiscFilePackage selectedPackage;

    public int[] DiscSizes { get; } = [700, 4480, 8500, 23500];

    [RelayCommand]
    private void SetDiscSize(int size)
    {
        Config.DiscSizeMB = size;
    }

    protected override Task OnInitializedAsync()
    {
        var pkgs = Utility.Packages.DiscFilePackages;
        if (Utility.Packages.SizeOutOfRangeFiles.Count > 0)
        {
            pkgs.Add(new DiscFilePackage()
            {
                Index = -1
            });
            pkgs[^1].Files.AddRange(Utility.Packages.SizeOutOfRangeFiles);
        }

        DiscFilePackages = pkgs;
        return base.OnInitializedAsync();
    }

    protected override async Task OnExecutingAsync(CancellationToken token)
    {
        if (!Enumerable.Any<DiscFilePackage>(DiscFilePackages, p => p.IsChecked))
        {
            throw new Exception("没有任何被选中的文件包");
        }

        if (Directory.Exists(Config.TargetDir) && Directory.EnumerateFileSystemEntries(Config.TargetDir).Any())
        {
            var result = await this.SendMessage(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.YesNo,
                Title = "清空目录",
                Message = $"目录{Config.TargetDir}不为空，{Environment.NewLine}导出前将清空部分目录。{Environment.NewLine}是否继续？"
            }).Task;
            if (true.Equals(result))
            {
                try
                {
                    foreach (var index in Utility.Packages.DiscFilePackages.Where(p => p.IsChecked)
                                 .Select(p => p.Index))
                    {
                        var dir = Path.Combine(Config.TargetDir, index.ToString());
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir,true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("清空目录失败", ex);
                }
            }
            else
            {
                throw new OperationCanceledException();
            }
        }
    }

    protected override async Task OnExecutedAsync(CancellationToken token)
    {
        if (Enumerable.Where<DiscFilePackage>(DiscFilePackages, p=>p.IsChecked)
            .Any(p => p.Files.Any(q => q.Status==ProcessStatus.Error)))
        {
            await this.ShowErrorAsync("导出可能失败","部分文件导出失败，请检查");
        }

    }

    protected override void OnReset()
    {
        DiscFilePackages = null;
    }

    [RelayCommand]
    private void SelectAll()
    {
        DiscFilePackages.ForEach(p => p.IsChecked = true);
    }

    [RelayCommand]
    private void SelectNone()
    {
        DiscFilePackages.ForEach(p => p.IsChecked = false);
    }
}