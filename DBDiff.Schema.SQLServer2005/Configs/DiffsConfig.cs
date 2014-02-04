using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDiff.Schema.SQLServer.Generates.Configs
{
    /// <summary>
    /// This class has somewhat different style to make JSON fields names have
    /// traditional underscore_lower_case than typical .NET CamelCase.
    /// </summary>
    public class DiffsConfig
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

        /// <summary>
        /// Returns default config that sets everything to "true".
        /// </summary>
        /// <returns></returns>
        public static DiffsConfig GetDefault()
        {
            return new DiffsConfig()
            {
                tables = true,
                assemblies = true,
                user_data_types = true,
                xml_schemas = true,
                schemas = true,
                file_groups = true,
                rules = true,
                ddl_triggers = true,
                synonyms = true,
                users = true,
                stored_procedures = true,
                clr_stored_procedures = true,
                clr_functions = true,
                views = true,
                functions = true,
                roles = true,
                partition_functions = true,
                partition_schemes = true,
                table_types = true,
                full_text = true,

                // Within tables:
                columns = true,
                constraints = true,
                table_options = true,
                triggers = true,
                clr_triggers = true,
                full_text_indexes = true,
                indexes = true,
            };
        }
    }

}
