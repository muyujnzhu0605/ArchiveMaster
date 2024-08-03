using System.Collections;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.UI.ViewModels;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib;
using FzLib.Avalonia.Dialogs;
using FzLib.Avalonia.Messages;

namespace DiscArchivingTool;

public partial class PackingPanelViewModel : TwoStepViewModelBase
{
    public PackingPanelViewModel()
    {
        DiscSizeMB = Config.DiscSizeMB;
    }

    [ObservableProperty]
    private List<DiscFilePackage> discFilePackages;

    [ObservableProperty]
    private DateTime earliestDateTime = new DateTime(1, 1, 1);

    [ObservableProperty]
    private int discSizeMB;
    
    

    partial void OnDiscSizeMBChanged(int value)
    {
        Config.DiscSizeMB = value;
    }

    [ObservableProperty]
    private DiscFilePackage selectedPackage;

    public int[] DiscSizes { get; } = new int[] { 700, 4480, 8500, 23500 };

    [RelayCommand]
    private void SetDiscSize(int size)
    {
        Config.DiscSizeMB = size;
        this.Notify(nameof(Config));
    }

    public IEnumerable PackingTypes => Enum.GetValues(typeof(PackingType));

    public PackingConfig Config { get; } = AppConfig.Instance.Get<PackingConfig>();
    private PackingUtility u;

    protected override async Task InitializeImplAsync()
    {
        if (string.IsNullOrWhiteSpace(Config.SourceDir))
        {
            throw new Exception("源目录为空");
        }

        if (!Directory.Exists(Config.SourceDir))
        {
            throw new Exception("源目录不存在");
        }

        if (Config.DiscSizeMB < 100)
        {
            throw new Exception("单盘容量过小");
        }

        if (Config.MaxDiscCount < 1)
        {
            throw new Exception("盘片数量应大于等于1盘");
        }

        // Config.SourceDir = SourceDir;
        // Config.TargetDir = TargetDir;

        u = new PackingUtility(Config);
        u.ProgressUpdate += Utility_ProgressUpdate;
        await u.InitializeAsync();

        var pkgs = u.Packages.DiscFilePackages;
        if (u.Packages.SizeOutOfRangeFiles.Count > 0)
        {
            pkgs.Add(new DiscFilePackage()
            {
                Index = -1
            });
            pkgs[^1].Files.AddRange(u.Packages.SizeOutOfRangeFiles);
        }

        DiscFilePackages = pkgs;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        if (!DiscFilePackages.Any(p => p.IsChecked))
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
                    foreach (var index in u.Packages.DiscFilePackages.Where(p => p.IsChecked).Select(p => p.Index))
                    {
                        var dir = Path.Combine(Config.TargetDir, index.ToString());
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir);
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
                return;
            }
        }

        await u.ExecuteAsync(token);
        u.ProgressUpdate -= Utility_ProgressUpdate;
    }

    protected override void ResetImpl()
    {
        DiscFilePackages = null;
    }
}