﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBDiff.Schema.SQLServer.Generates.Model;
using DBDiff.Schema.Model;
using DBDiff.Schema.SQLServer.Generates.Configs;

namespace DBDiff.Schema.SQLServer.Generates.Compare
{
    internal class ComparePartitionSchemes : CompareBase<PartitionScheme>
    {
        protected override void DoUpdate<Root>(SchemaList<PartitionScheme, Root> CamposOrigen, PartitionScheme node, DiffsConfig config = null)
        {
            if (!PartitionScheme.Compare(node, CamposOrigen[node.FullName]))
            {
                PartitionScheme newNode = node;//.Clone(CamposOrigen.Parent);
                newNode.Status = Enums.ObjectStatusType.RebuildStatus;
                CamposOrigen[node.FullName] = newNode;
            }
        }
    }
}
