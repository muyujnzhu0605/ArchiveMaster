using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ArchiveMaster.Configs
{
    public class AppConfig
    {
        private const string configFile = "configs.json";

        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
        };

        private List<ConfigItem> configs = new List<ConfigItem>();
        private Dictionary<string, Type> configTypes = new Dictionary<string, Type>();

        public event EventHandler BeforeSaving;

        public bool DebugMode { get; set; }

        public int DebugModeLoopDelay { get; set; } = 30;

        public Exception LoadError { get; private set; }

        public T GetConfig<T>(string key, string group=null, string version=null) where T:new()
        {
            var configItem = configs.FirstOrDefault(p => p.Key == key && p.Group == group && p.Version == version);
            if (configItem == null)
            {
                configItem = new ConfigItem
                {
                    Key = key,
                    Group = group,
                    Version = version,
                    Type = typeof(T),
                    Config = new T()
                };
                configs.Add(configItem);
            }
            else
            {
                if (configItem.Config is not T)
                {
                    throw new InvalidCastException(
                        $"配置列表中的配置项{key}（组：{group ?? "（无）"}；版本：{version ?? "（无）"}）的配置项对象类型（{configItem.Type?.Name}）与请求的类型（{typeof(T).Name}）不一致");
                }
            }

            return (T)configItem.Config;
        }

        public void Load()
        {
            try
            {
                Dictionary<string, object> fileConfigs = new Dictionary<string, object>();
                if (File.Exists(configFile))
                {
                    var json = JsonNode.Parse(File.ReadAllText(configFile));
                    if (json is not JsonArray jarray)
                    {
                        throw new Exception("配置文件内不是Json Array");
                    }

                    foreach (var key in configTypes.Keys)
                    {
                        var configItemJsons =
                            jarray.Where(p => key.Equals(p[nameof(ConfigInfo.Key)]?.GetValue<string>()));
                        foreach (var j in configItemJsons)
                        {
                            ConfigItem configItem = new ConfigItem
                            {
                                Key = key,
                                Type = configTypes[key],
                                Version = j[nameof(configItem.Version)]?.GetValue<string>(),
                                Group = j[nameof(configItem.Group)]?.GetValue<string>(),
                                Config = j[nameof(configItem.Config)]?.Deserialize(configTypes[key])
                            };
                            configs.Add(configItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载配置失败");
                LoadError = new Exception("加载配置文件失败，将重置配置", ex);
            }
        }

        public void RegisterConfig(ConfigInfo config)
        {
            configTypes.Add(config.Key, config.Type);
        }

        public void Save(bool raiseEvent = true)
        {
            if (raiseEvent)
            {
                BeforeSaving?.Invoke(this, EventArgs.Empty);
            }

            try
            {
                var json = JsonSerializer.Serialize(configs, jsonOptions);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
            }
        }
    }
}