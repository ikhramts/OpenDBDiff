using System;
using System.Collections.Generic;
using System.Text;

using DBDiff.Schema.Model;

namespace DBDiff.Schema.SQLServer.Generates.Model
{
    public class RowData : SQLServerSchemaBase
    {
        private readonly Dictionary<string, object> _columnValues;
        private readonly string _fullName;


        public RowData(ISchemaBase parent, Dictionary<string, object> columnValues)
            : base(parent, Enums.ObjectType.RowData)
        {
            _columnValues = columnValues;

            var fullNameBuilder = new StringBuilder();
            fullNameBuilder.Append("Row: ");

            foreach (var keyValue in columnValues)
            {
                fullNameBuilder.Append(keyValue.Key)
                               .Append('=')
                               .Append(keyValue.Value)
                               .Append('|');
            }

            _fullName = fullNameBuilder.ToString();
        }

        public override int GetHashCode()
        {
            int hash = 17;

            foreach (var keyValue in _columnValues)
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
            return DictionariesEqual(_columnValues, other._columnValues);
        }

        public override string FullName
        {
            get
            {
                return _fullName;
            }
        }

        public override string ToSql()
        {
            // Assume that the parent is a table.
            var sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", Parent.FullName);
            var first = true;

            foreach (var keyValue in _columnValues)
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
            foreach (var keyValue in _columnValues)
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

            foreach (var keyValue in _columnValues)
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

        public override SQLScript Create()
        {
            return new SQLScript(this.ToSql(), 0, Enums.ScripActionType.InsertRow);
        }

        public override SQLScript Drop()
        {
            return new SQLScript(this.ToSqlDrop(), 0, Enums.ScripActionType.DeleteRow);
        }

        public override SQLScriptList ToSqlDiff()
        {
            var list = new SQLScriptList();

            if (this.Status != Enums.ObjectStatusType.OriginalStatus)
                RootParent.ActionMessage[Parent.FullName].Add(this);

            if (this.HasState(Enums.ObjectStatusType.DropStatus))
            {
                if (this.Parent.Status != Enums.ObjectStatusType.RebuildStatus)
                    list.Add(Drop());
            }

            if (this.HasState(Enums.ObjectStatusType.CreateStatus))
                list.Add(Create());

            if (this.HasState(Enums.ObjectStatusType.AlterStatus))
            {
                list.Add(Drop());
                list.Add(Create());
            }

            return list;
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
                return String.Format("'{0}'", value);
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
