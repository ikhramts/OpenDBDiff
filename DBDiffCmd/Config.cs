using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBDiff.OCDB
{
    /// <summary>
    /// Stores database diff generation options.  Can also load the config
    /// from yaml file using Load() function.
    /// </summary>
    class Config
    {
        public string connection_string1 { get; set; }
        public string connection_string2 { get; set; }
        public string output_file { get; set; }

        public class IncludedDiffs
        {

        }
    }
}
