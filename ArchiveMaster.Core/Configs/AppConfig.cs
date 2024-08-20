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

        public bool DebugMode { get; set; }
#if DEBUG
            = true;
#endif
        public int DebugModeLoopDelay { get; set; } = 30;

        public static void RegisterConfig(ConfigInfo config)
        {
            configs.Add(config.Key,config);
        }

        public void Load()
        {
            if (isLoaded)
            {
                return;
            }

            try
            {
                Dictionary<string, object> fileConfigs = new Dictionary<string, object>();
                if (File.Exists(configFile))
                {
                    var json = File.ReadAllText(configFile);
                    JsonObject jobj = JsonNode.Parse(json) as JsonObject;
                    foreach (var kv in jobj)
                    {
                        if (configs.TryGetValue(kv.Key, out ConfigInfo config))
                        {
                            fileConfigs.Add(kv.Key, JsonSerializer.Deserialize(kv.Value, config.Type));
                            //
                            // config.Config = JsonSerializer.Deserialize(kv.Value, config.Type);
                            // Services.Builder.AddSingleton(config.Type,  config.Config);
                        }
                    }
                }

                foreach (var config in configs.Values)
                {
                    if (fileConfigs.ContainsKey(config.Key))
                    {
                        Services.Builder.AddSingleton(config.Type, fileConfigs[config.Key]);
                    }
                    else
                    {
                        Services.Builder.AddSingleton(config.Type);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            isLoaded = true;
        }

        public void Save(bool raiseEvent = true)
        {
            if (raiseEvent)
            {
                BeforeSaving?.Invoke(this, EventArgs.Empty);
            }

            try
            {
                var json = JsonSerializer.Serialize(
                    configs.Values.ToDictionary(p => p.Key, p => Services.Provider.GetRequiredService(p.Type)),
                    jsonOptions);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
            }
        }
    }
}