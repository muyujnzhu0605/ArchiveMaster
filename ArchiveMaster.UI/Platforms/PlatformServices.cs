namespace ArchiveMaster.Platforms
{
    public class PlatformServices
    {
        public static IPermissionService Permissions { get; set; }
        public static IViewService ViewService { get; set; }
        public static IBackCommandService BackCommand { get; set; }
    }
}
