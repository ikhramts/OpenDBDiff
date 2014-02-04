using DBDiff.Schema.SQLServer.Generates.Configs;
using DBDiff.Schema.SQLServer.Generates.Model;
using DBDiff.Schema.Model;

namespace DBDiff.Schema.SQLServer.Generates.Compare
{
    internal class CompareTables:CompareBase<Table>
    {
        protected override void DoUpdate<Root>(SchemaList<Table, Root> originFields, Table node, DiffsConfig config = null)
        {
            if (config == null)
            {
                config = DiffsConfig.GetDefault();
            }

            if (node.Status != Enums.ObjectStatusType.DropStatus)
            {
                Table originTable = originFields[node.FullName];
                originTable.OriginalTable = (Table)originFields[node.FullName].Clone((Database)originTable.Parent);
                if (config.columns) (new CompareColumns()).GenerateDiferences<Table>(originTable.Columns, node.Columns);
                if (config.constraints) (new CompareConstraints()).GenerateDiferences<Table>(originTable.Constraints, node.Constraints);
                if (config.indexes) (new CompareIndexes()).GenerateDiferences<Table>(originTable.Indexes, node.Indexes);
                if (config.table_options) (new CompareTablesOptions()).GenerateDiferences<Table>(originTable.Options, node.Options);
                if (config.triggers) (new CompareTriggers()).GenerateDiferences<Table>(originTable.Triggers, node.Triggers);
                if (config.clr_triggers) (new CompareCLRTriggers()).GenerateDiferences<Table>(originTable.CLRTriggers, node.CLRTriggers);
                if (config.full_text_indexes) (new CompareFullTextIndex()).GenerateDiferences<Table>(originTable.FullTextIndex, node.FullTextIndex);
                
                if (config.file_groups
                    && (!Table.CompareFileGroup(originTable, node)))
                {
                    originTable.FileGroup = node.FileGroup;
                    /* This only applies to heap tables, the rest does the field in the clustered index filegroup */
                    if (!originTable.HasClusteredIndex)
                        originTable.Status = Enums.ObjectStatusType.RebuildStatus;
                }
                
                if (config.file_groups
                    && (!Table.CompareFileGroupText(originTable, node)))
                {
                    originTable.FileGroupText = node.FileGroupText;
                    originTable.Status = Enums.ObjectStatusType.RebuildStatus;
                }
                
                if (node.HasChangeTracking != originTable.HasChangeTracking)
                {
                    originTable.HasChangeTracking = node.HasChangeTracking;
                    originTable.HasChangeTrackingTrackColumn = node.HasChangeTrackingTrackColumn;
                    originTable.Status += (int)Enums.ObjectStatusType.DisabledStatus;
                }
            }
        }

        /// <summary>
        /// Compara las colecciones de tablas de dos bases diferentes y marca el estado de los objetos
        /// dependiendo si existen o si deben borrarse.
        /// </summary>
        /// <param name="tablasOrigen"></param>
        /// Tablas originales, donde se guardaran los estados de las tablas.
        /// <param name="tablasDestino">
        /// Tablas comparativas, que se usa para comparar con la base original.
        /// </param>
        /*public static void GenerateDiferences(SchemaList<Table, Database> tablasOrigen, SchemaList<Table, Database> tablasDestino)
        {
            MarkDrop(tablasOrigen, tablasDestino);

            foreach (Table node in tablasDestino)
            {
                if (!tablasOrigen.Exists(node.FullName))
                {
                    node.Status = Enums.ObjectStatusType.CreateStatus;
                    node.Parent = tablasOrigen.Parent; 
                    tablasOrigen.Add(node);
                }
                else
                {
                    if (node.Status != Enums.ObjectStatusType.DropStatus)
                    {
                        Table tablaOriginal = tablasOrigen[node.FullName];
                        tablaOriginal.OriginalTable = (Table)tablasOrigen[node.FullName].Clone((Database)tablaOriginal.Parent);
                        CompareColumns.GenerateDiferences<Table>(tablaOriginal.Columns, node.Columns);
                        CompareConstraints.GenerateDiferences<Table>(tablaOriginal.Constraints, node.Constraints);
                        CompareIndexes.GenerateDiferences(tablaOriginal.Indexes, node.Indexes);
                        CompareTablesOptions.GenerateDiferences(tablaOriginal.Options, node.Options);
                        (new CompareTriggers()).GenerateDiferences<Table>(tablaOriginal.Triggers, node.Triggers);
                        (new CompareCLRTriggers()).GenerateDiferences<Table>(tablaOriginal.CLRTriggers, node.CLRTriggers);
                        if (!Table.CompareFileGroup(tablaOriginal, node))
                        {
                            tablaOriginal.FileGroup = node.FileGroup;
                            //Esto solo aplica a las tablas heap, el resto hace el campo en el filegroup del indice clustered
                            if (!tablaOriginal.HasClusteredIndex)
                                tablaOriginal.Status = Enums.ObjectStatusType.RebuildStatus;
                        }
                        if (!Table.CompareFileGroupText(tablaOriginal, node))
                        {
                            tablaOriginal.FileGroupText = node.FileGroupText;
                            tablaOriginal.Status = Enums.ObjectStatusType.RebuildStatus;
                        }
                    }
                }
            }                       
        }*/
    }
}
