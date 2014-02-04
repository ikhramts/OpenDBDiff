using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DBDiff.Schema.SQLServer.Generates.Configs;
using DBDiff.Schema.SQLServer.Generates.Model;
using DBDiff.Schema.Misc;

namespace DBDiff.Schema.SQLServer.Generates.Compare
{
    internal static class CompareDatabase
    {        
        public static Database GenerateDiferences(Database origin, Database destination, DiffsConfig config = null)
        {
            if (config == null)
            {
                config = DiffsConfig.GetDefault();
            }

            try
            {
                Database data = origin;
                if (config.tables) (new CompareTables()).GenerateDiferences<Database>(origin.Tables, destination.Tables, config);
                if (config.assemblies) (new CompareAssemblies()).GenerateDiferences<Database>(origin.Assemblies, destination.Assemblies);
                if (config.user_data_types) (new CompareUserDataTypes()).GenerateDiferences<Database>(origin.UserTypes, destination.UserTypes);
                if (config.xml_schemas) (new CompareXMLSchemas()).GenerateDiferences<Database>(origin.XmlSchemas, destination.XmlSchemas);
                if (config.schemas) (new CompareSchemas()).GenerateDiferences<Database>(origin.Schemas, destination.Schemas);
                if (config.file_groups) (new CompareFileGroups()).GenerateDiferences<Database>(origin.FileGroups, destination.FileGroups);
                if (config.rules) (new CompareRules()).GenerateDiferences<Database>(origin.Rules, destination.Rules);
                if (config.ddl_triggers) (new CompareDDLTriggers()).GenerateDiferences<Database>(origin.DDLTriggers, destination.DDLTriggers);
                if (config.synonyms) (new CompareSynonyms()).GenerateDiferences<Database>(origin.Synonyms, destination.Synonyms);
                if (config.users) (new CompareUsers()).GenerateDiferences<Database>(origin.Users, destination.Users);
                if (config.stored_procedures) (new CompareStoreProcedures()).GenerateDiferences<Database>(origin.Procedures, destination.Procedures);
                if (config.clr_stored_procedures) (new CompareCLRStoreProcedure()).GenerateDiferences<Database>(origin.CLRProcedures, destination.CLRProcedures);
                if (config.clr_functions) (new CompareCLRFunction()).GenerateDiferences<Database>(origin.CLRFunctions, destination.CLRFunctions);
                if (config.views) (new CompareViews()).GenerateDiferences<Database>(origin.Views, destination.Views);
                if (config.functions) (new CompareFunctions()).GenerateDiferences<Database>(origin.Functions, destination.Functions);
                if (config.roles) (new CompareRoles()).GenerateDiferences<Database>(origin.Roles, destination.Roles);
                if (config.partition_functions) (new ComparePartitionFunction()).GenerateDiferences<Database>(origin.PartitionFunctions, destination.PartitionFunctions);
                if (config.partition_schemes) (new ComparePartitionSchemes()).GenerateDiferences<Database>(origin.PartitionSchemes, destination.PartitionSchemes);
                if (config.table_types) (new CompareTableType()).GenerateDiferences<Database>(origin.TablesTypes, destination.TablesTypes);
                if (config.full_text) (new CompareFullText()).GenerateDiferences<Database>(origin.FullText, destination.FullText);

                data.SourceInfo = destination.Info;
                return data;
            }
            catch (SchemaException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SchemaException(ex.Message,ex);
            }
        }
    }
}
