using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DBDiff.Schema.Errors;
using DBDiff.Schema.SQLServer.Generates.Configs;
using DBDiff.Schema.SQLServer.Generates.Model;

namespace DBDiff.Schema.SQLServer.Generates.Generates
{
    public class GenerateRowData
    {
        private Generate root;

        public GenerateRowData(Generate root)
        {
            this.root = root;
        }

        public void Fill(Database database,
                         string connectionString,
                         List<MessageLog> messages,
                         DiffsConfig config = null)
        {
            if (config == null)
            {
                config = new DiffsConfig();
            }

            foreach (var table in database.Tables)
            {
                LoadTableRows(table, connectionString, messages, config);
            }
            
        }

        public void LoadTableRows(Table table,
                                  string connectionString,
                                  List<MessageLog> messages,
                                  DiffsConfig config)
        {
            // Check whether the table data should be loaded.
            var tableName = table.FullName;
            var shouldLoadRows = false;

            foreach (var pattern in config.data_in_tables)
            {
                if (Regex.IsMatch(tableName, pattern))
                {
                    shouldLoadRows = true;
                    break;
                }
            }

            if (!shouldLoadRows)
                return;

            // Load the table data.
            var sql = String.Format("SELECT TOP {0} * FROM {1}", config.max_rows_to_diff + 1, tableName);
            DataTable rowsTable = null;

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    var dataAdapter = new SqlDataAdapter(command);
                    rowsTable = new DataTable();
                    dataAdapter.Fill(rowsTable);
                }
            }

            if (rowsTable.Rows.Count > config.max_rows_to_diff)
            {
                var description = String.Format("Table {0}: too many rows to diff", table.FullName);
                var fullDescription = String.Format("The number of rows in the table {0} has exceeded "
                                                    + "the maximum number of rows allowed in row diff "
                                                    + "({1:N0} rows).",
                                                    table.FullName, config.max_rows_to_diff);
                var message = new MessageLog(description, fullDescription, MessageLog.LogType.Warning);
                messages.Add(message);
                return;
            }

            // Package the rows.
            var columns = rowsTable.Columns;

            foreach (DataRow row in rowsTable.Rows)
            {
                var rowValues = new Dictionary<string, object>();

                foreach (DataColumn column in columns)
                {
                    var value = row[column.ColumnName];
                    rowValues["[" + column.ColumnName + "]"] = value;
                }

                var rowData = new RowData(table, rowValues);
                table.Rows.Add(rowData);
            }
        }
    }
}
