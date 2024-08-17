using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ArchiveMaster.Platforms;
using Avalonia;
using Avalonia.Android;
using FzLib.Avalonia.Platforms;
using System;
using System.Diagnostics;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace ArchiveMaster.UI.Android;

[Activity(
    Label = "文件归档大师",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>, IPermissionService, IStorageService, IBackCommandService
{
    private const int REQUEST_MANAGE_EXTERNAL_STORAGE = 1024;
    private const int REQUEST_READ_AND_WRITE_EXTERNAL_STORAGE = 1025;

    private Func<bool> backAction;

    public void CheckPermissions()
    {
        if ((int)Build.VERSION.SdkInt >= (int)BuildVersionCodes.R)
        {
            if (!Environment.IsExternalStorageManager)
            {
                Toast.MakeText(this, "本应用需要访问存储器权限", ToastLength.Short).Show();

                Intent intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission);
                intent.SetData(Uri.Parse("package:" + PackageName));
                StartActivityForResult(intent, REQUEST_MANAGE_EXTERNAL_STORAGE);
            }

        }
    }

    public string GetExternalFilesDir()
    {
        var dir = GetExternalFilesDir(string.Empty);
        return dir.AbsolutePath.Split(["Android"], StringSplitOptions.None)[0];
    }

    public override void OnBackPressed()
    {
        bool? result = backAction?.Invoke();
        if (result != true)
        {
            base.OnBackPressed();
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    public void RegisterBackCommand(Func<bool> backAction)
    {
        this.backAction = backAction;
    }
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Platforms.PlatformServices.Permissions = this;
        Platforms.PlatformServices.BackCommand = this;
        FzLib.Avalonia.Platforms.PlatformServices.StorageService = this;
        return base.CustomizeAppBuilder(builder);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode == REQUEST_MANAGE_EXTERNAL_STORAGE)
        {
            if (Environment.IsExternalStorageManager)
            {
                // 权限已授予
                Toast.MakeText(this, "权限已授予", ToastLength.Short).Show();
                RequestPermissions([Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage], REQUEST_READ_AND_WRITE_EXTERNAL_STORAGE);
            }
            else
            {
                // 权限未授予
                Toast.MakeText(this, "权限未授予，无此权限本应用无法运行，请授予", ToastLength.Short).Show();
                CheckPermissions();
            }
        }
        else
        {

        }
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
        Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
        Window.SetStatusBarColor(Color.Transparent);
    }
}
