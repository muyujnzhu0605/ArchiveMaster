namespace ArchiveMaster.Configs;

public class ConfigItem : ConfigMetadata
{
    public static ConfigItem FromConfigMetadata(ConfigMetadata metadata,object config,string version)
    {
        return new ConfigItem
        {
            Key = metadata.Key,
            Type = metadata.Type,
            Group = metadata.Group,
            Config = config,
            Version = version
        };
    }
    
    /// <summary>
    /// 配置对象
    /// </summary>
    public object Config { get; set; }

    /// <summary>
    /// 启用多版本配置时的版本号
    /// </summary>
    public string Version { get; set; }
}