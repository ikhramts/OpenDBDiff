using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DBDiff.Schema.Model;

namespace DBDiff.Schema.SQLServer.Generates.Model
{
    public class RowData : SQLServerSchemaBase
    {
        private Dictionary<string, object> columnValues;
        private string fullName;


        public RowData(ISchemaBase parent, Dictionary<string, object> columnValues)
            : base(parent, Enums.ObjectType.RowData)
        {
            this.columnValues = columnValues;

            var fullNameBuilder = new StringBuilder();
            fullNameBuilder.Append("Row: ");

            foreach (var keyValue in columnValues)
            {
                fullNameBuilder.Append(keyValue.Key)
                               .Append('=')
                               .Append(keyValue.Value.ToString())
                               .Append('|');
            }

            fullName = fullNameBuilder.ToString();
        }

        public override int GetHashCode()
        {
            int hash = 17;

            foreach (var keyValue in columnValues)
            {
                hash += 23 * keyValue.Key.GetHashCode();
                hash += 23 * keyValue.Value.GetHashCode();
            }

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is RowData))
            {
                return false;
            }

            var other = (RowData)obj;
            return DictionariesEqual(this.columnValues, other.columnValues);
        }

        public override string FullName
        {
            get
            {
                return fullName;
            }
        }

        public override string ToSql()
        {
            // Assume that the parent is a table.
            var sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", Parent.FullName);
            var first = true;

            foreach (var keyValue in columnValues)
            {
                if (first)
                    first = false;
                else
                    sql.Append(", ");

                var column = keyValue.Key;
                sql.Append(column);
            }

            sql.Append(")\n    VALUES (");

            first = true;
            foreach (var keyValue in columnValues)
            {
                if (first)
                    first = false;
                else
                    sql.Append(", ");

                var value = keyValue.Value;
                sql.Append(ValueToSql(value));
            }

            sql.Append(");\nGO\n");
            return sql.ToString();
        }

        public override string ToSqlAdd()
        {
            return ToSql();
        }

        public override string ToSqlDrop()
        {
            var sql = new StringBuilder();

            sql.AppendFormat("DELETE TOP 1 FROM {0} WHERE", Parent.FullName);
            var first = true;

            foreach (var keyValue in columnValues)
            {
                sql.Append("\n    ");

                if (first)
                    first = false;
                else
                    sql.Append("AND ");

                var column = keyValue.Key;
                var value = keyValue.Value;

                sql.Append(column);

                if (value is DBNull)
                {
                    sql.Append(" is NULL");
                }
                else
                {
                    sql.Append(" = ").Append(ValueToSql(value));
                }
            }

            sql.Append("\nGO\n");

            return sql.ToString();
        }

        private static bool DictionariesEqual<TKey, TValue>(
            IDictionary<TKey, TValue> first, 
            IDictionary<TKey, TValue> second)
        {
            if (first == second) return true;
            if ((first == null) || (second == null)) return false;
            if (first.Count != second.Count) return false;

            var comparer = EqualityComparer<TValue>.Default;

            foreach (KeyValuePair<TKey, TValue> kvp in first)
            {
                TValue secondValue;
                if (!second.TryGetValue(kvp.Key, out secondValue)) return false;
                if (!comparer.Equals(kvp.Value, secondValue)) return false;
            }

            return true;
        }

        private static string ValueToSql(object value)
        {
            if (value is DBNull)
            {
                return "NULL";
            }
            else if (value is DateTime)
            {
                return String.Format("'{0:yyyy-MM-dd HH:mm:ss.fff}'", (DateTime)value);
            }
            else if (value is string)
            {
                return String.Format("'{0}'", (string)value);
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
