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

        private Dictionary<string, ConfigInfo> configs = new Dictionary<string, ConfigInfo>();

        public event EventHandler BeforeSaving;

        public bool DebugMode { get; set; }

        // #if DEBUG
        //             = true;
        // #endif

        public int DebugModeLoopDelay { get; set; } = 30;

        public Exception LoadError { get; private set; }

        public void Load(IServiceCollection services)
        {
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
                    if (fileConfigs.TryGetValue(config.Key, out object obj) && obj != null)
                    {
                        try
                        {
                            services.AddSingleton(config.Type, obj);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"加载单个配置{config.Key}失败");
                        }
                    }
                    else
                    {
                        services.AddSingleton(config.Type);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载配置失败");
                LoadError = new Exception("加载配置文件失败，将重置配置", ex);
                foreach (var config in configs.Values)
                {
                    services.AddSingleton(config.Type);
                }
            }
        }

        public void RegisterConfig(ConfigInfo config)
        {
            configs.Add(config.Key, config);
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
                    configs.Values.ToDictionary(p => p.Key, p => HostServices.GetRequiredService(p.Type)),
                    jsonOptions);
                File.WriteAllText(configFile, json);
            }
            catch (Exception ex)
            {
            }
        }
    }
}