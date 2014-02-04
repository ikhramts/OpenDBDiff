using DBDiff.Schema.SQLServer.Generates.Model;
using DBDiff.Schema.Model;
using DBDiff.Schema.SQLServer.Generates.Configs;

namespace DBDiff.Schema.SQLServer.Generates.Compare
{
    internal class CompareCLRStoreProcedure : CompareBase<CLRStoreProcedure>
    {
        protected override void DoUpdate<Root>(SchemaList<CLRStoreProcedure, Root> CamposOrigen, CLRStoreProcedure node, DiffsConfig config = null)
        {
            if (!node.Compare(CamposOrigen[node.FullName]))
            {
                CLRStoreProcedure newNode = node;//.Clone(CamposOrigen.Parent);
                newNode.Status = Enums.ObjectStatusType.AlterStatus;
                CamposOrigen[node.FullName] = newNode;
            }
        }
    }
}
