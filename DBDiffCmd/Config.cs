using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace DBDiff.OCDB
{
    /// <summary>
    /// Stores database diff generation options.  Can also load the config
    /// from yaml file using Load() function.
    /// 
    /// This class has somewhat different style to make JSON fields names have
    /// traditional underscore_lower_case than typical .NET CamelCase.
    /// </summary>
    class Config
    {
        public string connection_string_from { get; set; }
        public string connection_string_to { get; set; }
        public string output_file { get; set; }
        public IncludedDiffs generate_diffs { get; set; }

        public class IncludedDiffs
        {
            public bool tables { get; set; }
            public bool assemblies { get; set; }
            public bool user_data_types { get; set; }
            public bool xml_schemas { get; set; }
            public bool schemas { get; set; }
            public bool file_groups { get; set; }
            public bool rules { get; set; }
            public bool ddl_triggers { get; set; }
            public bool synonyms { get; set; }
            public bool users { get; set; }
            public bool stored_procedures { get; set; }
            public bool clr_stored_procedures { get; set; }
            public bool clr_functions { get; set; }
            public bool views { get; set; }
            public bool functions { get; set; }
            public bool roles { get; set; }
            public bool partition_functions { get; set; }
            public bool partition_schemes { get; set; }
            public bool table_types { get; set; }
            public bool full_text { get; set; }

            // Within tables:
            public bool columns { get; set; }
            public bool constraints { get; set; }
            public bool table_options { get; set; }
            public bool triggers { get; set; }
            public bool clr_triggers { get; set; }
            public bool full_text_indexes { get; set; }
            public bool indexes { get; set; }
        }

        public static Config LoadYaml(string configPath)
        {
            //System.Diagnostics.Debugger.Break();
            var configText = System.IO.File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<Config>(configText);

            return config;
        }
    }
}
