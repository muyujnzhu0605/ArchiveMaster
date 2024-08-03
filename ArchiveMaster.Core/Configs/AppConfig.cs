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

        private static Dictionary<string, ConfigInfo> configs = new Dictionary<string, ConfigInfo>();
        private bool isLoaded = false;
        public event EventHandler BeforeSaving;

        public static AppConfig Instance { get; } = new AppConfig();

        public static void RegisterConfig(Type type, string key)
        {
            configs.Add(key, new ConfigInfo(type: type, key: key));
        }

        public T Get<T>() where T : class
        {
            string key = typeof(T).Name;
            return Get(key) as T;
        }

        public object Get(string key)
        {
            if (!isLoaded)
            {
                Load();
            }

            if (configs.TryGetValue(key, out ConfigInfo config))
            {
                config.Config ??= Activator.CreateInstance(config.Type);
                return config.Config;
            }

            throw new Exception($"找不到对应的配置：{key}");
        }

        public void Load()
        {
            if (isLoaded)
            {
                return;
            }

            try
            {
                if (File.Exists(configFile))
                {
                    var json = File.ReadAllText(configFile);
                    JsonObject jobj = JsonNode.Parse(json) as JsonObject;
                    foreach (var kv in jobj)
                    {
                        if (configs.TryGetValue(kv.Key, out ConfigInfo config))
                        {
                            config.Config = JsonSerializer.Deserialize(kv.Value, config.Type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            isLoaded = true;
        }

        public void Save()
        {
            BeforeSaving?.Invoke(this, EventArgs.Empty);
            try
            {
                var json = JsonSerializer.Serialize(configs.Values.ToDictionary(p => p.Key, p => p.Config),
                    jsonOptions);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
            }
        }

        public void Set<T>(object value) where T : class
        {
            Set(typeof(T).Name, value);
        }


        public void Set(string key, object value)
        {
            if (!isLoaded)
            {
                Load();
            }

            if (!configs.TryGetValue(key, out ConfigInfo config))
            {
                throw new Exception("找不到对应的配置");
            }

            config.Config = value;
        }
    }
}