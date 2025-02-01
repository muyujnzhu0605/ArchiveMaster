using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using FzLib.Collection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ArchiveMaster.Configs
{
    /// <summary>
    /// 配置管理中心
    /// </summary>
    /// <remarks>
    /// 每个模块通过<see cref="RegisterConfig"/>上报自己所需要的配置，然后通过<see cref="GetOrCreateConfig{T}"/>获取配置。
    /// 配置基本信息为<see cref="ConfigMetadata"/>，运行后增加配置对象和版本，使用<see cref="ConfigItem"/>存储。
    /// 支持多配置功能，或称为多预设，即能够保存多个配置项，在程序中命名为版本（Version）。
    /// 通常来说，配置项之间的版本是独立的，一个配置增删改版本，不影响其他配置项。
    /// 但可以通过设置组来实现多个配置之间共享版本，一个典型的例子是OfflineSync，模块中的多个板块是步骤的关系，因此需要共享版本。
    /// 共享版本时，提供相同的组名，此时增删改其中一个配置项的版本，会同步反映到其他配置项上。
    /// 启用共享版本时，需要在IModuleInfo中提供Configs时指定每个Config使用相同的Group，并且在TwoStepViewModel初始化时提供Group。
    /// </remarks>
    public class AppConfig
    {
        public const string DEFAULT_VERSION = "默认";

        private const string JKEY_MODULES = "Modules";

        private const string JKEY_GROUPS = "Groups";

        private const string CONFIG_FILE = "configs.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
        };

        /// <summary>
        /// 模块配置
        /// </summary>
        private readonly List<ConfigItem> configs = new List<ConfigItem>();

        /// <summary>
        /// 模块配置的元数据
        /// </summary>
        private readonly Dictionary<string, ConfigMetadata> configMetadata = new Dictionary<string, ConfigMetadata>();

        /// <summary>
        /// 各配置组的当前版本
        /// </summary>
        private Dictionary<string, string> currentVersions = new Dictionary<string, string>();

        public event EventHandler BeforeSaving;

        public bool DebugMode { get; set; }

        public int DebugModeLoopDelay { get; set; } = 30;

        public Exception LoadError { get; private set; }

        public string GetCurrentVersion(string groupName)
        {
            ArgumentException.ThrowIfNullOrEmpty(groupName);
            return currentVersions.GetValueOrDefault(groupName, DEFAULT_VERSION);
        }

        public IReadOnlyList<string> GetVersions(string groupName)
        {
            ArgumentException.ThrowIfNullOrEmpty(groupName);
            var sameGroupConfigs = configMetadata.Values
                .Where(p => p.Group == groupName)
                .Select(p => p.Key)
                .ToHashSet();
            if (sameGroupConfigs.Count == 0)
            {
                throw new Exception($"没有组名为{groupName}的配置文件");
            }

            var versions = configs
                .Where(p => sameGroupConfigs.Contains(p.Key))
                .Select(p => p.Version)
                .Distinct()
                .ToList();
            if (versions.Count == 0)
            {
                versions.Add(DEFAULT_VERSION);
            }

            return versions.AsReadOnly();
        }

        public void SetCurrentVersion(string groupName, string version)
        {
            ArgumentException.ThrowIfNullOrEmpty(groupName);
            ArgumentException.ThrowIfNullOrEmpty(version);
            currentVersions[groupName] = version;
        }

        public T GetOrCreateConfig<T>(string key, string version = null) where T : new()
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ConfigItem configItem = configs.FirstOrDefault(p => p.Key == key && p.Version == version);
            if (!configMetadata.TryGetValue(key, out ConfigMetadata metadata))
            {
                throw new Exception($"没有注册名为{key}的配置");
            }

            if (configItem == null)
            {
                configItem = ConfigItem.FromConfigMetadata(metadata, new T(), version);
                configs.Add(configItem);
            }
            else
            {
                if (configItem.Config is not T)
                {
                    throw new InvalidCastException(
                        $"配置列表中的配置项{key}（版本：{version ?? "（无）"}）的配置项对象类型（{configItem.Type?.Name}）与请求的类型（{typeof(T).Name}）不一致");
                }
            }

            return (T)configItem.Config;
        }

        public void Initialize()
        {
            try
            {
                if (File.Exists(CONFIG_FILE))
                {
                    var json = JsonNode.Parse(File.ReadAllText(CONFIG_FILE));
                    if (json is not JsonObject jobj)
                    {
                        throw new Exception("配置文件内不是Json Object");
                    }

                    if (!jobj.ContainsKey(JKEY_MODULES))
                    {
                        throw new Exception($"配置文件内不包含{JKEY_MODULES}");
                    }

                    if (!jobj.ContainsKey(JKEY_GROUPS))
                    {
                        throw new Exception($"配置文件内不包含{JKEY_GROUPS}");
                    }

                    if (jobj[JKEY_GROUPS] is not JsonObject jg)
                    {
                        throw new Exception($"配置文件内的{JKEY_GROUPS}不是Json Object");
                    }

                    if (jobj[JKEY_MODULES] is not JsonArray jm)
                    {
                        throw new Exception($"配置文件内的{JKEY_MODULES}不是Json Array");
                    }

                    ParseModules(jm);
                    ParseGroups(jg);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载配置失败");
                LoadError = new Exception("加载配置文件失败，将重置配置", ex);
            }
        }

        private void ParseGroups(JsonObject jobj)
        {
            Debug.Assert(jobj!=null);
            currentVersions = jobj.Deserialize<Dictionary<string, string>>();
        }

        private void ParseModules(JsonArray jarray)
        {
            Debug.Assert(jarray!=null);
            foreach (var key in configMetadata.Keys)
            {
                var configItemJsons =
                    jarray.Where(p => key.Equals(p[nameof(ConfigMetadata.Key)]?.GetValue<string>()));
                foreach (var j in configItemJsons)
                {
                    var version = j[nameof(ConfigItem.Version)]?.GetValue<string>();
                    var instance = j[nameof(ConfigItem.Config)]?.Deserialize(configMetadata[key].Type);
                    configs.Add(ConfigItem.FromConfigMetadata(configMetadata[key], instance, version));
                }
            }
        }

        public void RegisterConfig(ConfigMetadata config)
        {
            ArgumentNullException.ThrowIfNull(config);
            configMetadata.Add(config.Key, config);
        }

        public void Save(bool raiseEvent = true)
        {
            if (raiseEvent)
            {
                BeforeSaving?.Invoke(this, EventArgs.Empty);
            }

            try
            {
                var json = JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    [JKEY_MODULES] = configs,
                    [JKEY_GROUPS] = currentVersions
                }, JsonOptions);
                File.WriteAllText(CONFIG_FILE, json);
            }
            catch (Exception ex)
            {
            }
        }

        public bool RemoveVersion<T>(string key, string version) where T : ConfigBase, new()
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentException.ThrowIfNullOrEmpty(version);
            var configItems = configs.Where(p => p.Key == key && p.Version == version).ToList();
            if (configItems.Count == 0)
            {
                return false;
            }

            foreach (var configItem in configItems)
            {
                configs.Remove(configItem);
            }

            //考虑设置当前版本号
            return true;
        }

        public void RenameVersion(string group, string oldName, string newName)
        {
            ArgumentException.ThrowIfNullOrEmpty(group);
            ArgumentException.ThrowIfNullOrEmpty(oldName);
            ArgumentException.ThrowIfNullOrEmpty(newName);
            foreach (var config in configs.Where(p => p.Group == group && p.Version == oldName))
            {
                config.Version = newName;
            }
        }
    }
}