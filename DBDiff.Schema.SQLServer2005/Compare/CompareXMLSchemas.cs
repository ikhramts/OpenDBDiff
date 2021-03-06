using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DBDiff.Schema.SQLServer.Generates.Model;
using DBDiff.Schema.Model;
using DBDiff.Schema.SQLServer.Generates.Configs;

namespace DBDiff.Schema.SQLServer.Generates.Compare
{
    internal class CompareXMLSchemas:CompareBase<XMLSchema>
    {
        protected override void DoUpdate<Root>(SchemaList<XMLSchema, Root> CamposOrigen, XMLSchema node, DiffsConfig config = null)
        {
            if (!node.Compare(CamposOrigen[node.FullName]))
            {
                XMLSchema newNode = node.Clone(CamposOrigen.Parent);
                newNode.Status = Enums.ObjectStatusType.AlterStatus;
                CamposOrigen[node.FullName] = newNode;
            }
        }
    }
}
