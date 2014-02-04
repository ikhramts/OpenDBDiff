using System;
using System.Data.SqlClient;
using System.IO;
using DBDiff.Schema.SQLServer.Generates.Configs;
using DBDiff.Schema.SQLServer.Generates.Generates;
using DBDiff.Schema.SQLServer.Generates.Options;

namespace DBDiff.OCDB
{
    public class Program
    {
        private static SqlOption SqlFilter = new SqlOption();

        static int Main(string[] args)
        {
            bool completedSuccessfully = false;
            try
            {
                Argument arguments = new Argument(args);
                if (arguments.Validate())
                    completedSuccessfully = Work(arguments);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return completedSuccessfully ? 0 : 1;
        }

        static Boolean TestConnection(string connectionString1, string connectionString2)
        {
            try
            {
                SqlConnection connection = new SqlConnection();
                connection.ConnectionString = connectionString1;
                connection.Open();
                connection.Close();
                connection.ConnectionString = connectionString2;
                connection.Open();
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static bool Work(Argument arguments)
        {
            bool completedSuccessfully = false;

            // Attempt to read the config file.
            Config config = null;

            if (arguments.ConfigFile != null)
            {
                System.Console.WriteLine("Reading config file...");
                config = Config.LoadJson(arguments.ConfigFile);

                if (String.IsNullOrEmpty(arguments.ConnectionString1))
                {
                    arguments.ConnectionString1 = config.connection_string_from;
                }

                if (String.IsNullOrEmpty(arguments.ConnectionString2))
                {
                    arguments.ConnectionString2 = config.connection_string_to;
                }

                if (String.IsNullOrEmpty(arguments.OutputFile))
                {
                    arguments.OutputFile = config.output_file;
                }
            }

            try
            {
                DBDiff.Schema.SQLServer.Generates.Model.Database origin;
                DBDiff.Schema.SQLServer.Generates.Model.Database destination;

                if (TestConnection(arguments.ConnectionString1, arguments.ConnectionString2))
                {
                    Generate sql = new Generate();
                    sql.ConnectionString = arguments.ConnectionString1;
                    System.Console.WriteLine("Reading first database...");
                    sql.Options = SqlFilter;
                    origin = sql.Process(config.generate_diffs);

                    sql.ConnectionString = arguments.ConnectionString2;
                    System.Console.WriteLine("Reading second database...");
                    destination = sql.Process(config.generate_diffs);
                    System.Console.WriteLine("Comparing databases schemas...");
                    origin = Generate.Compare(origin, destination, config.generate_diffs);
                    if (!arguments.OutputAll)
                    {
                        // temporary work-around: run twice just like GUI
                        origin.ToSqlDiff();
                    }

                    System.Console.WriteLine("Generating SQL file...");
                    SaveFile(arguments.OutputFile, arguments.OutputAll ? origin.ToSql() : origin.ToSqlDiff().ToSQL());
                    completedSuccessfully = true;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(String.Format("{0}\r\n{1}\r\n\r\nPlease report this issue at http://opendbiff.codeplex.com/workitem/list/basic\r\n\r\n", ex.Message, ex.StackTrace));
            }

            return completedSuccessfully;
        }

        static void SaveFile(string filenmame, string sql)
        {
            if (!String.IsNullOrEmpty(filenmame))
            {
                StreamWriter writer = new StreamWriter(filenmame, false);
                writer.Write(sql);
                writer.Close();
            }
        }
    }
}
