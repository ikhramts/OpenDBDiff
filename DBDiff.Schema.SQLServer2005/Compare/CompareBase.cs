using System;
using System.Collections.Generic;
using System.Linq;
using DBDiff.Schema.Model;
using DBDiff.Schema.SQLServer.Generates.Configs;
using DBDiff.Schema.SQLServer.Generates.Generates;
using DBDiff.Schema.SQLServer.Generates.Model;

namespace DBDiff.Schema.SQLServer.Generates.Compare
{
    internal abstract class CompareBase<T> where T:ISchemaBase
    {
        protected virtual void DoUpdate<Root>(SchemaList<T, Root> originFields, T node, DiffsConfig config = null) where Root : ISchemaBase
        {

        }

        protected virtual void DoNew<Root>(SchemaList<T, Root> originFields, T node) where Root : ISchemaBase
        {
            T newNode = node;//.Clone(CamposOrigen.Parent);
            newNode.Parent = originFields.Parent;
            newNode.Status = Enums.ObjectStatusType.CreateStatus;
            originFields.Add(newNode);
        }

        protected void DoDelete(T node)
        {
            node.Status = Enums.ObjectStatusType.DropStatus;
        }

        public void GenerateDiferences<Root>(SchemaList<T, Root> originFields, 
                                             SchemaList<T, Root> destinationFields,
                                             DiffsConfig config = null) where Root : ISchemaBase
        {
            int destinationIndex = 0;
            int originIndex = 0;
            int destinationCount = destinationFields.Count;
            int originCount = originFields.Count;
            T node;

            bool hasFieldsRemaining = true;

            while (hasFieldsRemaining)
            {
                hasFieldsRemaining = false;

                if (destinationCount > destinationIndex)
                {
                    node = destinationFields[destinationIndex];
                    Generate.RaiseOnCompareProgress("Comparing Destination {0}: [{1}]", node.ObjectType, node.Name);

                    if (!originFields.Exists(node.FullName))
                    {
                        Generate.RaiseOnCompareProgress("Adding {0}: [{1}]", node.ObjectType, node.Name);
                        DoNew<Root>(originFields, node);
                    }
                    else
                    {
                        Generate.RaiseOnCompareProgress("Updating {0}: [{1}]", node.ObjectType, node.Name);
                        DoUpdate<Root>(originFields, node, config);
                    }

                    destinationIndex++;
                    hasFieldsRemaining = true;
                }

                if (originCount > originIndex)
                {
                    node = originFields[originIndex];
                    Generate.RaiseOnCompareProgress("Comparing Source {0}: [{1}]", node.ObjectType, node.Name);

                    if (!destinationFields.Exists(node.FullName))
                    {
                        Generate.RaiseOnCompareProgress("Deleting {0}: [{1}]", node.ObjectType, node.Name);
                        DoDelete(node);
                    }

                    originIndex++;
                    hasFieldsRemaining = true;
                }
            }
        }

        protected static void CompareExtendedProperties(ISQLServerSchemaBase origen, ISQLServerSchemaBase destino)
        {
            List<ExtendedProperty> dropList = (from node in origen.ExtendedProperties
                                               where !destino.ExtendedProperties.Exists(item => item.Name.Equals(node.Name, StringComparison.CurrentCultureIgnoreCase))
                                               select node).ToList<ExtendedProperty>();
            List<ExtendedProperty> addList = (from node in destino.ExtendedProperties
                                              where !origen.ExtendedProperties.Exists(item => item.Name.Equals(node.Name, StringComparison.CurrentCultureIgnoreCase))
                                               select node).ToList<ExtendedProperty>();
            dropList.ForEach(item => { item.Status = Enums.ObjectStatusType.DropStatus;} );
            addList.ForEach(item => { item.Status = Enums.ObjectStatusType.CreateStatus; });
            origen.ExtendedProperties.AddRange(addList);
        }
    }
}
