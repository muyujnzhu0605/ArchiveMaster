using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Widget;
using ArchiveMaster.Platforms;
using Avalonia;
using Avalonia.Android;

namespace ArchiveMaster.UI.Android;

[Activity(
    Label = "ArchiveMaster.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>, IPermissionService
{
    private const int REQUEST_MANAGE_EXTERNAL_STORAGE = 1024;
    private const int REQUEST_READ_AND_WRITE_EXTERNAL_STORAGE = 1025;

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

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        PlatformService.Permissions = this;
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
}
