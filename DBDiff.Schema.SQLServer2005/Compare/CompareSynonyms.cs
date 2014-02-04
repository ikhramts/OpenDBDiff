using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBDiff.Schema.SQLServer.Generates.Model;
using DBDiff.Schema.Model;
using DBDiff.Schema.SQLServer.Generates.Configs;

namespace DBDiff.Schema.SQLServer.Generates.Compare
{
    internal class CompareSynonyms:CompareBase<Synonym>
    {
        protected override void DoUpdate<Root>(SchemaList<Synonym, Root> CamposOrigen, Synonym node, DiffsConfig config = null)
        {
            if (!Synonym.Compare(node, CamposOrigen[node.FullName]))
            {
                Synonym newNode = node;//.Clone(CamposOrigen.Parent);
                newNode.Status = Enums.ObjectStatusType.AlterStatus;
                CamposOrigen[node.FullName] = newNode;
            }
        }
    }
}
