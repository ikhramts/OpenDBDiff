using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
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
        /// This field specifies in which tables we should diff
        /// the data.  Each string in this list should contain a 
        /// regex; if the table name matches this regex, DBDiff
        /// will calculate the data diff in the table.
        /// </summary>
        public List<string> data_in_tables { get; private set; }

        /// <summary>
        /// Notwithstanding data_in_tables property, do not perform 
        /// data diff on the table which has more than this number 
        /// of rows.
        /// </summary>
        public int max_rows_to_diff { get; set; }

        /// <summary>
        /// The default config is to diff everything except for data,
        /// and to set the maximum number of rows to do data diff on
        /// to 1000.
        /// </summary>
        public DiffsConfig()
        {
            tables = true;
            assemblies = true;
            user_data_types = true;
            xml_schemas = true;
            schemas = true;
            file_groups = true;
            rules = true;
            ddl_triggers = true;
            synonyms = true;
            users = true;
            stored_procedures = true;
            clr_stored_procedures = true;
            clr_functions = true;
            views = true;
            functions = true;
            roles = true;
            partition_functions = true;
            partition_schemes = true;
            table_types = true;
            full_text = true;

            // Within tables:
            columns = true;
            constraints = true;
            table_options = true;
            triggers = true;
            clr_triggers = true;
            full_text_indexes = true;
            indexes = true;

            // Data diff settings.
            data_in_tables = new List<string>();
            max_rows_to_diff = 1000;
        }

        public bool ShouldDiffTableRows(string tableFullName)
        {
            foreach (var pattern in data_in_tables)
            {
                if (Regex.IsMatch(tableFullName, pattern))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// By default, diff everything except for data.
        /// </summary>
        /// <returns></returns>
        public static DiffsConfig GetDefault()
        {
            return new DiffsConfig();
        }
    }

}
