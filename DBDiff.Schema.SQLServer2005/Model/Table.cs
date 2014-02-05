using System;
using System.Collections.Generic;
using System.Linq;
using DBDiff.Schema.Attributes;
using DBDiff.Schema.Model;

namespace DBDiff.Schema.SQLServer.Generates.Model
{
    public class Table : SQLServerSchemaBase, IComparable<Table>, ITable<Table>
    {
        private Columns<Table> _columns;
        private int _dependenciesCount;
        private List<ISchemaBase> _dependencies;
        private Boolean? _hasFileStream;

        public Table(ISchemaBase parent)
            : base(parent, Enums.ObjectType.Table)
        {
            _dependenciesCount = -1;
            _columns = new Columns<Table>(this);
            Constraints = new SchemaList<Constraint, Table>(this, ((Database) parent).AllObjects);
            Options = new SchemaList<TableOption, Table>(this);
            Triggers = new SchemaList<Trigger, Table>(this, ((Database) parent).AllObjects);
            ClrTriggers = new SchemaList<CLRTrigger, Table>(this, ((Database) parent).AllObjects);
            Indexes = new SchemaList<Index, Table>(this, ((Database) parent).AllObjects);
            Partitions = new SchemaList<TablePartition, Table>(this, ((Database) parent).AllObjects);
            FullTextIndex = new SchemaList<FullTextIndex, Table>(this);
            Rows = new SchemaList<RowData, Table>(this);
        }

        public string CompressType { get; set; }

        public string FileGroupText { get; set; }

        public Boolean HasChangeDataCapture { get; set; }

        public Boolean HasChangeTrackingTrackColumn { get; set; }

        public Boolean HasChangeTracking { get; set; }

        public string FileGroupStream { get; set; }

        public Boolean HasClusteredIndex { get; set; }

        public string FileGroup { get; set; }

        public Table OriginalTable { get; set; }

        [ShowItem("Constraints")]
        public SchemaList<Constraint, Table> Constraints { get; private set; }

        [ShowItem("Indexes", "Index")]
        public SchemaList<Index, Table> Indexes { get; private set; }

        [ShowItem("CLR Triggers")]
        public SchemaList<CLRTrigger, Table> ClrTriggers { get; private set; }

        [ShowItem("Triggers")]
        public SchemaList<Trigger, Table> Triggers { get; private set; }

        public SchemaList<FullTextIndex, Table> FullTextIndex { get; private set; }

        public SchemaList<TablePartition, Table> Partitions { get; set; }

        public SchemaList<TableOption, Table> Options { get; set; }

        public SchemaList<RowData, Table> Rows { get; set; }

        /// <summary>
        /// Indicates if the table has a column that is Identity.
        /// </summary>
        public Boolean HasIdentityColumn
        {
            get { return Columns.Any(col => col.IsIdentity); }
        }

        public Boolean HasFileStream
        {
            get
            {
                if (_hasFileStream == null)
                {
                    _hasFileStream = false;
                    foreach (Column col in Columns)
                    {
                        if (col.IsFileStream) _hasFileStream = true;
                    }
                }
                return _hasFileStream.Value;
            }
        }

        public Boolean HasBlobColumn
        {
            get { return Columns.Any(col => col.IsBLOB); }
        }

        /// <summary>
        /// Indicates the number of dependents Constraints in another table (FK) having
        /// Table.
        /// </summary>
        public override int DependenciesCount
        {
            get
            {
                if (_dependenciesCount == -1)
                    _dependenciesCount = ((Database) Parent).Dependencies.DependenciesCount(Id,
                                                                                           Enums.ObjectType.Constraint);
                return _dependenciesCount;
            }
        }

        #region IComparable<Table> Members

        /// <summary>
        /// Compare first order Operation
        /// (First go Drops, then the Create and Alter finalesmente).
        /// If the operation is the same, sort by number of dependent tables.
        /// </summary>
        public int CompareTo(Table other)
        {
            if (other == null) throw new ArgumentNullException("other");
            if (Status == other.Status)
                return DependenciesCount.CompareTo(other.DependenciesCount);
            return other.Status.CompareTo(Status);
        }

        #endregion

        #region ITable<Table> Members

        /// <summary>
        /// Collection of table fields.
        /// </summary>
        [ShowItem("Columns", "Column")]
        public Columns<Table> Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        #endregion

        /// <summary>
        /// Clones the Table object to a new instance.
        /// </summary>
        public override ISchemaBase Clone(ISchemaBase objectParent)
        {
            var table = new Table(objectParent)
                            {
                                Owner = Owner,
                                Name = Name,
                                Id = Id,
                                Guid = Guid,
                                Status = Status,
                                FileGroup = FileGroup,
                                FileGroupText = FileGroupText,
                                FileGroupStream = FileGroupStream,
                                HasClusteredIndex = HasClusteredIndex,
                                HasChangeTracking = HasChangeTracking,
                                HasChangeTrackingTrackColumn = HasChangeTrackingTrackColumn,
                                HasChangeDataCapture = HasChangeDataCapture,
                                _dependenciesCount = DependenciesCount
                            };
            table.Columns = Columns.Clone(table);
            table.Options = Options.Clone(table);
            table.CompressType = CompressType;
            table.Triggers = Triggers.Clone(table);
            table.Indexes = Indexes.Clone(table);
            table.Partitions = Partitions.Clone(table);
            return table;
        }

        public override string ToSql()
        {
            return ToSql(true);
        }

        /// <summary>
        /// Returns the schema of the table in SQL format.
        /// </summary>
        public string ToSql(Boolean showFK)
        {
            Database database = null;
            ISchemaBase current = this;

            while (database == null && current.Parent != null)
            {
                database = current.Parent as Database;
                current = current.Parent;
            }

            var isAzure10 = database.Info.Version == DatabaseInfo.VersionTypeEnum.SQLServerAzure10;
                
            string sql = "";
            string sqlPrinaryKey = "";
            string sqlUniqueConstraint = "";
            string sqlForeignKey = "";

            if (_columns.Count > 0)
            {
                sql += "CREATE TABLE " + FullName + "\r\n(\r\n";
                sql += _columns.ToSql();
                if (Constraints.Count > 0)
                {
                    sql += ",\r\n";
                    Constraints.ForEach(item =>
                                            {
                                                if (item.Type == Constraint.ConstraintType.PrimaryKey)
                                                    sqlPrinaryKey += "\t" + item.ToSql() + ",\r\n";
                                                if (item.Type == Constraint.ConstraintType.Unique)
                                                    sqlUniqueConstraint += "\t" + item.ToSql() + ",\r\n";
                                                if (showFK)
                                                    if (item.Type == Constraint.ConstraintType.ForeignKey)
                                                        sqlForeignKey += "\t" + item.ToSql() + ",\r\n";
                                            });
                    sql += sqlPrinaryKey + sqlUniqueConstraint + sqlForeignKey;
                    sql = sql.Substring(0, sql.Length - 3) + "\r\n";
                }
                else
                {
                    sql += "\r\n";
                    if (!String.IsNullOrEmpty(CompressType))
                        sql += "WITH (DATA_COMPRESSION = " + CompressType + ")\r\n";
                }
                sql += ")";
                
                if (!isAzure10)
                {
                    if (!String.IsNullOrEmpty(FileGroup)) sql += " ON [" + FileGroup + "]";
                
                    if (!String.IsNullOrEmpty(FileGroupText))
                    {
                        if (HasBlobColumn)
                            sql += " TEXTIMAGE_ON [" + FileGroupText + "]";
                    }
                    if ((!String.IsNullOrEmpty(FileGroupStream)) && (HasFileStream))
                        sql += " FILESTREAM_ON [" + FileGroupStream + "]";
                }
                sql += "\r\n";
                sql += "GO\r\n";
                Constraints.ForEach(item =>
                                        {
                                            if (item.Type == Constraint.ConstraintType.Check)
                                                sql += item.ToSqlAdd() + "\r\n";
                                        });
                if (HasChangeTracking)
                    sql += ToSqlChangeTracking();
                sql += Indexes.ToSql();
                sql += FullTextIndex.ToSql();
                sql += Options.ToSql();
                sql += Triggers.ToSql();
                sql += Rows.ToSql();
            }
            return sql;
        }

        private string ToSqlChangeTracking()
        {
            string sql;
            if (HasChangeTracking)
            {
                sql = "ALTER TABLE " + FullName + " ENABLE CHANGE_TRACKING";
                if (HasChangeTrackingTrackColumn)
                    sql += " WITH(TRACK_COLUMNS_UPDATED = ON)";
            }
            else
                sql = "ALTER TABLE " + FullName + " DISABLE CHANGE_TRACKING";

            return sql + "\r\nGO\r\n";
        }

        public override string ToSqlAdd()
        {
            return ToSql();
        }

        public override string ToSqlDrop()
        {
            return "DROP TABLE " + FullName + "\r\nGO\r\n";
        }

/*
        private SQLScriptList BuildSQLFileGroup()
        {
            var listDiff = new SQLScriptList();

            Boolean found = false;
            Index clustered = Indexes.Find(item => item.Type == Index.IndexTypeEnum.Clustered);
            if (clustered == null)
            {
                foreach (Constraint cons in Constraints)
                {
                    if (cons.Index.Type == Index.IndexTypeEnum.Clustered)
                    {
                        listDiff.Add(cons.ToSqlDrop(FileGroup), dependenciesCount, Enums.ScripActionType.DropConstraint);
                        listDiff.Add(cons.ToSqlAdd(), dependenciesCount, Enums.ScripActionType.AddConstraint);
                        found = true;
                    }
                }
                if (!found)
                {
                    Status = Enums.ObjectStatusType.RebuildStatus;
                    listDiff = ToSqlDiff();
                }
            }
            else
            {
                listDiff.Add(clustered.ToSqlDrop(FileGroup), dependenciesCount, Enums.ScripActionType.DropIndex);
                listDiff.Add(clustered.ToSqlAdd(), dependenciesCount, Enums.ScripActionType.AddIndex);
            }
            return listDiff;
        }
*/

        /// <summary>
        /// Returns the schema of the table of differences in SQL format.
        /// </summary>
        public override SQLScriptList ToSqlDiff()
        {
            var listDiff = new SQLScriptList();

            if (Status != Enums.ObjectStatusType.OriginalStatus)
            {
                if (((Database) Parent).Options.Ignore.FilterTable)
                    RootParent.ActionMessage.Add(this);
            }

            if (Status == Enums.ObjectStatusType.DropStatus)
            {
                if (((Database) Parent).Options.Ignore.FilterTable)
                {
                    listDiff.Add(ToSqlDrop(), _dependenciesCount, Enums.ScripActionType.DropTable);
                    listDiff.AddRange(ToSQLDropFKBelow());
                }
            }
            if (Status == Enums.ObjectStatusType.CreateStatus)
            {
                string sql = "";
                Constraints.ForEach(item =>
                                        {
                                            if (item.Type == Constraint.ConstraintType.ForeignKey)
                                                sql += item.ToSqlAdd() + "\r\n";
                                        });
                listDiff.Add(ToSql(false), _dependenciesCount, Enums.ScripActionType.AddTable);
                listDiff.Add(sql, _dependenciesCount, Enums.ScripActionType.AddConstraintFK);
            }
            if (HasState(Enums.ObjectStatusType.RebuildDependenciesStatus))
            {
                GenerateDependencies();
                listDiff.AddRange(ToSQLDropDependencis());
                listDiff.AddRange(_columns.ToSqlDiff());
                listDiff.AddRange(ToSQLCreateDependencis());
                listDiff.AddRange(Constraints.ToSqlDiff());
                listDiff.AddRange(Indexes.ToSqlDiff());
                listDiff.AddRange(Options.ToSqlDiff());
                listDiff.AddRange(Triggers.ToSqlDiff());
                listDiff.AddRange(ClrTriggers.ToSqlDiff());
                listDiff.AddRange(FullTextIndex.ToSqlDiff());
                listDiff.AddRange(Rows.ToSqlDiff());
            }
            if (HasState(Enums.ObjectStatusType.AlterStatus))
            {
                listDiff.AddRange(_columns.ToSqlDiff());
                listDiff.AddRange(Constraints.ToSqlDiff());
                listDiff.AddRange(Indexes.ToSqlDiff());
                listDiff.AddRange(Options.ToSqlDiff());
                listDiff.AddRange(Triggers.ToSqlDiff());
                listDiff.AddRange(ClrTriggers.ToSqlDiff());
                listDiff.AddRange(FullTextIndex.ToSqlDiff());
                listDiff.AddRange(Rows.ToSqlDiff());
            }
            if (HasState(Enums.ObjectStatusType.RebuildStatus))
            {
                GenerateDependencies();
                listDiff.AddRange(ToSQLRebuild());
                listDiff.AddRange(_columns.ToSqlDiff());
                listDiff.AddRange(Constraints.ToSqlDiff());
                listDiff.AddRange(Indexes.ToSqlDiff());
                listDiff.AddRange(Options.ToSqlDiff());
                // As recreates the table, just put the new triggers, why will not ToSQLDiff ToSQL.
                listDiff.Add(Triggers.ToSql(), _dependenciesCount, Enums.ScripActionType.AddTrigger);
                listDiff.Add(ClrTriggers.ToSql(), _dependenciesCount, Enums.ScripActionType.AddTrigger);
                listDiff.AddRange(FullTextIndex.ToSqlDiff());
                listDiff.AddRange(Rows.ToSqlDiff());
            }
            if (HasState(Enums.ObjectStatusType.DisabledStatus))
            {
                listDiff.Add(ToSqlChangeTracking(), 0, Enums.ScripActionType.AlterTableChangeTracking);
            }
            return listDiff;
        }

        private string ToSQLTableRebuild()
        {
            string sql = "";
            string tempTable = "Temp" + Name;
            string listColumns = "";
            string listValues = "";
            var isIdentityNew = false;
            
            foreach (var column in Columns)
            {
                if ((column.Status != Enums.ObjectStatusType.DropStatus) &&
                    !((column.Status == Enums.ObjectStatusType.CreateStatus) && column.IsNullable))
                {
                    if ((!column.IsComputed) && (!column.Type.ToLower().Equals("timestamp")))
                    {
                        /*Si la nueva columna a agregar es XML, no se inserta ese campo y debe ir a la coleccion de Warnings*/
                        /*Si la nueva columna a agregar es Identity, tampoco se debe insertar explicitamente*/
                        if (
                            !((column.Status == Enums.ObjectStatusType.CreateStatus) &&
                              ((column.Type.ToLower().Equals("xml") || (column.IsIdentity)))))
                        {
                            listColumns += "[" + column.Name + "],";
                            if (column.HasToForceValue)
                            {
                                if (column.HasState(Enums.ObjectStatusType.UpdateStatus))
                                    listValues += "ISNULL([" + column.Name + "]," + column.DefaultForceValue + "),";
                                else
                                    listValues += column.DefaultForceValue + ",";
                            }
                            else
                                listValues += "[" + column.Name + "],";
                        }
                        else
                        {
                            if (column.IsIdentity) isIdentityNew = true;
                        }
                    }
                }
            }
            if (!String.IsNullOrEmpty(listColumns))
            {
                listColumns = listColumns.Substring(0, listColumns.Length - 1);
                listValues = listValues.Substring(0, listValues.Length - 1);
                sql += ToSQLTemp(tempTable) + "\r\n";
                if ((HasIdentityColumn) && (!isIdentityNew))
                    sql += "SET IDENTITY_INSERT [" + Owner + "].[" + tempTable + "] ON\r\n";
                sql += "INSERT INTO [" + Owner + "].[" + tempTable + "] (" + listColumns + ")" + " SELECT " +
                       listValues + " FROM " + FullName + "\r\n";
                if ((HasIdentityColumn) && (!isIdentityNew))
                    sql += "SET IDENTITY_INSERT [" + Owner + "].[" + tempTable + "] OFF\r\nGO\r\n\r\n";
                sql += "DROP TABLE " + FullName + "\r\nGO\r\n";

                if (HasFileStream)
                {
                    Constraints.ForEach(item =>
                                            {
                                                if ((item.Type == Constraint.ConstraintType.Unique) &&
                                                    (item.Status != Enums.ObjectStatusType.DropStatus))
                                                {
                                                    sql += "EXEC sp_rename N'[" + Owner + "].[Temp_XX_" + item.Name +
                                                           "]',N'" + item.Name + "', 'OBJECT'\r\nGO\r\n";
                                                }
                                            });
                }
                sql += "EXEC sp_rename N'[" + Owner + "].[" + tempTable + "]',N'" + Name +
                       "', 'OBJECT'\r\nGO\r\n\r\n";
                sql += OriginalTable.Options.ToSql();
            }
            else
                sql = "";
            return sql;
        }

        private SQLScriptList ToSQLRebuild()
        {
            var listDiff = new SQLScriptList();
            listDiff.AddRange(ToSQLDropDependencis());
            listDiff.Add(ToSQLTableRebuild(), _dependenciesCount, Enums.ScripActionType.RebuildTable);
            listDiff.AddRange(ToSQLCreateDependencis());
            return listDiff;
        }

        private string ToSQLTemp(String tableName)
        {
            string sql = "";
            sql += "CREATE TABLE [" + Owner + "].[" + tableName + "]\r\n(\r\n";

            Columns.Sort();

            for (int index = 0; index < Columns.Count; index++)
            {
                if (Columns[index].Status != Enums.ObjectStatusType.DropStatus)
                {
                    sql += "\t" + Columns[index].ToSql(true);
                    if (index != Columns.Count - 1)
                        sql += ",";
                    sql += "\r\n";
                }
            }
            if (HasFileStream)
            {
                sql = sql.Substring(0, sql.Length - 2);
                sql += ",\r\n";
                Constraints.ForEach(item =>
                                        {
                                            if ((item.Type == Constraint.ConstraintType.Unique) &&
                                                (item.Status != Enums.ObjectStatusType.DropStatus))
                                            {
                                                item.Name = "Temp_XX_" + item.Name;
                                                sql += "\t" + item.ToSql() + ",\r\n";
                                                item.SetWasInsertInDiffList(Enums.ScripActionType.AddConstraint);
                                                item.Name = item.Name.Substring(8, item.Name.Length - 8);
                                            }
                                        });
                sql = sql.Substring(0, sql.Length - 3) + "\r\n";
            }
            else
            {
                sql += "\r\n";
                if (!String.IsNullOrEmpty(CompressType))
                    sql += "WITH (DATA_COMPRESSION = " + CompressType + ")\r\n";
            }

            sql += ")";

            if (!String.IsNullOrEmpty(FileGroup)) sql += " ON [" + FileGroup + "]";
            
            if (!String.IsNullOrEmpty(FileGroupText))
            {
                if (HasBlobColumn)
                    sql += " TEXTIMAGE_ON [" + FileGroupText + "]";
            }
            
            if ((!String.IsNullOrEmpty(FileGroupStream)) && (HasFileStream))
                sql += " FILESTREAM_ON [" + FileGroupStream + "]";

            sql += "\r\n";
            sql += "GO\r\n";
            return sql;
        }

        private void GenerateDependencies()
        {
            List<ISchemaBase> myDependencies;
            // If the status is AlterRebuildDependeciesStatus seeks dependencies 
            // only on the columns that were modified.
            if (Status == Enums.ObjectStatusType.RebuildDependenciesStatus)
            {
                myDependencies = new List<ISchemaBase>();
                for (int ic = 0; ic < Columns.Count; ic++)
                {
                    if ((Columns[ic].Status == Enums.ObjectStatusType.RebuildDependenciesStatus) ||
                        (Columns[ic].Status == Enums.ObjectStatusType.AlterStatus))
                        myDependencies.AddRange(((Database) Parent).Dependencies.Find(Id, 0, Columns[ic].DataUserTypeId));
                }

                // If none are found, making all of the table.
                if (myDependencies.Count == 0)
                    myDependencies.AddRange(((Database) Parent).Dependencies.Find(Id));
            }
            else
                myDependencies = ((Database) Parent).Dependencies.Find(Id);

            _dependencies = new List<ISchemaBase>();
            for (int j = 0; j < myDependencies.Count; j++)
            {
                ISchemaBase item = null;
                if (myDependencies[j].ObjectType == Enums.ObjectType.Index)
                    item = Indexes[myDependencies[j].FullName];
                if (myDependencies[j].ObjectType == Enums.ObjectType.Constraint)
                    item =
                        ((Database) Parent).Tables[myDependencies[j].Parent.FullName].Constraints[
                            myDependencies[j].FullName];
                if (myDependencies[j].ObjectType == Enums.ObjectType.Default)
                    item = _columns[myDependencies[j].FullName].DefaultConstraint;
                if (myDependencies[j].ObjectType == Enums.ObjectType.View)
                    item = ((Database) Parent).Views[myDependencies[j].FullName];
                if (myDependencies[j].ObjectType == Enums.ObjectType.Function)
                    item = ((Database) Parent).Functions[myDependencies[j].FullName];
                if (item != null)
                    _dependencies.Add(item);
            }
        }

        /// <summary>
        /// Generates a list of FK that must be removed prior to removal of the tables.
        /// This happens because in order to delete a table, you have to delete before 
        /// all the associated constraints.
        /// </summary>
        private SQLScriptList ToSQLDropFKBelow()
        {
            var listDiff = new SQLScriptList();
            Constraints.ForEach(constraint =>
                {
                    if ((constraint.Type == Constraint.ConstraintType.ForeignKey) &&
                        (((Table) constraint.Parent).DependenciesCount <= DependenciesCount))
                    {
                        // If the FK belongs to the same table, do not explain the DROP 
                        // CONSTRAINT prior to the DROP TABLE.
                        if (constraint.Parent.Id != constraint.RelationalTableId)
                        {
                            listDiff.Add(constraint.Drop());
                        }
                    }
                });
            return listDiff;
        }

        /// <summary>
        /// Generates a list of all script DROPS dependent constraints of the table.
        /// It is used when you want to rebuild a table and all its dependent objects.
        /// </summary>
        private SQLScriptList ToSQLDropDependencis()
        {
            var listDiff = new SQLScriptList();
            //Se buscan todas las table constraints.
            for (int index = 0; index < _dependencies.Count; index++)
            {
                if ((_dependencies[index].Status == Enums.ObjectStatusType.OriginalStatus) ||
                    (_dependencies[index].Status == Enums.ObjectStatusType.DropStatus))
                {
                    var addDependency = true;

                    if (_dependencies[index].ObjectType == Enums.ObjectType.Constraint)
                    {
                        if ((((Constraint) _dependencies[index]).Type == Constraint.ConstraintType.Unique) &&
                            ((HasFileStream) || (OriginalTable.HasFileStream)))
                            addDependency = false;
                        if ((((Constraint) _dependencies[index]).Type != Constraint.ConstraintType.ForeignKey) &&
                            (_dependencies[index].Status == Enums.ObjectStatusType.DropStatus))
                            addDependency = false;
                    }
                    if (addDependency)
                        listDiff.Add(_dependencies[index].Drop());
                }
            }
            // All columns are searched constraints.
            _columns.ForEach(column =>
                                {
                                    if (column.DefaultConstraint != null)
                                    {
                                        if (((column.DefaultConstraint.Status == Enums.ObjectStatusType.OriginalStatus) ||
                                             (column.DefaultConstraint.Status == Enums.ObjectStatusType.DropStatus) ||
                                             (column.DefaultConstraint.Status == Enums.ObjectStatusType.AlterStatus)) &&
                                            (column.Status != Enums.ObjectStatusType.CreateStatus))
                                            listDiff.Add(column.DefaultConstraint.Drop());
                                    }
                                });
            return listDiff;
        }

        private SQLScriptList ToSQLCreateDependencis()
        {
            var listDiff = new SQLScriptList();

            // The constraints of must travel in the reverse order.
            for (int index = _dependencies.Count - 1; index >= 0; index--)
            {
                if ((_dependencies[index].Status == Enums.ObjectStatusType.OriginalStatus) &&
                    (_dependencies[index].Parent.Status != Enums.ObjectStatusType.DropStatus))
                {
                    var addDependency = true;

                    if (_dependencies[index].ObjectType == Enums.ObjectType.Constraint)
                    {
                        if ((((Constraint) _dependencies[index]).Type == Constraint.ConstraintType.Unique) &&
                            (HasFileStream))
                            addDependency = false;
                    }
                    if (addDependency)
                        listDiff.Add(_dependencies[index].Create());
                }
            }
            // All columns are searched constraints.
            for (int index = _columns.Count - 1; index >= 0; index--)
            {
                if (_columns[index].DefaultConstraint != null)
                {
                    if ((_columns[index].DefaultConstraint.CanCreate) &&
                        (_columns.Parent.Status != Enums.ObjectStatusType.RebuildStatus))
                        listDiff.Add(_columns[index].DefaultConstraint.Create());
                }
            }
            return listDiff;
        }

        /// <summary>
        /// Compare two tables and returns true if they are equal, otherwise returns false.
        /// </summary>
        public static Boolean CompareFileGroup(Table origen, Table destino)
        {
            if (destino == null) throw new ArgumentNullException("destino");
            if (origen == null) throw new ArgumentNullException("origen");
            if ((!String.IsNullOrEmpty(destino.FileGroup) && (!String.IsNullOrEmpty(origen.FileGroup))))
                if (!destino.FileGroup.Equals(origen.FileGroup))
                    return false;
            return true;
        }

        /// <summary>
        /// Compare two tables and returns true if they are equal, otherwise returns false.
        /// </summary>
        public static Boolean CompareFileGroupText(Table origen, Table destino)
        {
            if (destino == null) throw new ArgumentNullException("destino");
            if (origen == null) throw new ArgumentNullException("origen");
            if ((!String.IsNullOrEmpty(destino.FileGroupText) && (!String.IsNullOrEmpty(origen.FileGroupText))))
                if (!destino.FileGroupText.Equals(origen.FileGroupText))
                    return false;
            return true;
        }
    }
}