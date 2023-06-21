using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FlowChart;
using FlowChart.Misc;

namespace NewFlowChartTool
{
    public class AppConfig
    {
        static AppConfig()
        {
            Inst = Load(CONFIG_FILENAME) ?? new AppConfig();
        }

        #region Config Property
        [JsonPropertyName("light_theme")]
        public bool LightTheme { get; set; }
        [JsonPropertyName("english")]
        public bool EnglishLang { get; set; }
        [JsonPropertyName("show_console")]
        public bool ShowConsole { get; set; }
        #endregion


        #region Load & Save

        private const string CONFIG_FILENAME = "tmp/app_config.json";
        public static AppConfig Inst { get; set; }
        
        public static AppConfig? Load(string fileName)
        {
            try
            {
                using (var fs = new FileStream(CONFIG_FILENAME, FileMode.Open))
                {
                    var cfg = JsonSerializer.Deserialize<AppConfig>(fs);
                    return cfg;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public static void Save()
        {
            try
            {
                var config = JsonSerializer.Serialize(Inst, new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    WriteIndented = true
                });
                System.IO.File.WriteAllText(CONFIG_FILENAME, config);
            }
            catch (Exception e)
            {
                Console.WriteLine($"save app config failed, {e.Message}");
            }
        }
        #endregion
    }
}
