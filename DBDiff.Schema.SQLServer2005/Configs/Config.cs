using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace DBDiff.Schema.SQLServer.Generates.Configs
{
    /// <summary>
    /// Stores database diff generation options.  Can also load the config
    /// from yaml file using Load() function.
    /// 
    /// This class has somewhat different style to make JSON fields names have
    /// traditional underscore_lower_case than typical .NET CamelCase.
    /// </summary>
    public class Config
    {
        public string connection_string_from { get; set; }
        public string connection_string_to { get; set; }
        public string output_file { get; set; }
        public DiffsConfig generate_diffs { get; set; }

        public static Config LoadYaml(string configPath)
        {
            //System.Diagnostics.Debugger.Break();
            var configText = System.IO.File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<Config>(configText);

            return config;
        }
    }
}
