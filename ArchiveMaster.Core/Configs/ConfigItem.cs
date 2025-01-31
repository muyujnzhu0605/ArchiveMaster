namespace ArchiveMaster.Configs;

public class ConfigItem : ConfigInfo
{
    /// <summary>
    /// 配置对象
    /// </summary>
    public object Config { get; set; }

    /// <summary>
    /// 启用多版本配置时的版本号
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 启用多版本配置时的版本组名。同一个组名内的配置共享一组版本号。
    /// </summary>
    /// <returns></returns>
    public string Group { get; set; }
}