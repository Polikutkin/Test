using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Resources;
using System.Linq;
using System.Text;

namespace WA_2554_PaymentMethodsUpdater
{
    class DatabaseDao
    {
        private string connectionString;
        private ResourceManager resourceMgr;

        public DatabaseDao()
        {
            connectionString = ConfigurationManager.ConnectionStrings["ISFrontOfficeConnectionString"].ConnectionString;
        }

        public bool WasFilialProcessed(Guid filialId)
        {
            var result = false;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = Resources.ProcName_WasFilialProcessed;
                        command.Parameters.AddWithValue("@FilialId", filialId);
                        connection.Open();
                        result = command.ExecuteScalar() != null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }

        internal void DropDatabaseObjects()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        connection.Open();
                        command.CommandText = Resources.Script_DropProc_AddFilial;
                        command.ExecuteNonQuery();

                        command.CommandText = Resources.Script_DropProc_GetParentIdByLogin;
                        command.ExecuteNonQuery();

                        command.CommandText = Resources.Script_DropProc_WasFilialProcessed;
                        command.ExecuteNonQuery();

                        command.CommandText = Resources.Script_DropTable_Filials;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        internal void CreateDatabaseObjects()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        connection.Open();
                        command.CommandText = Resources.Script_CreateTable_Filials;
                        command.ExecuteNonQuery();

                        command.CommandText = Resources.Script_CreateProc_AddFilial;
                        command.ExecuteNonQuery();
                        command.CommandText = Resources.Script_AlterProc_AddFilial;
                        command.ExecuteNonQuery();

                        command.CommandText = Resources.Script_CreateProc_GetparentIdByLogin;
                        command.ExecuteNonQuery();
                        command.CommandText = Resources.Script_AlterProc_GetparentIdByLogin;
                        command.ExecuteNonQuery();

                        command.CommandText = Resources.Script_CreateProc_WasFilialProcessed;
                        command.ExecuteNonQuery();
                        command.CommandText = Resources.Script_AlterProc_WasFilialProcessed;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        internal void UpdateAllProductsForFilial(Guid filialId, IEnumerable<Guid> products, string newValue)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = Resources.ProcName_AddOrUpdateProfileSetting;
                        command.Parameters.AddWithValue("@Key", Resources.Param_Key);
                        command.Parameters.AddWithValue("@Filial", filialId);
                        command.Parameters.AddWithValue("@DefaultValue", newValue);
                        command.Parameters.AddWithValue("@ValueType", Resources.Param_Value_Type);
                        command.Parameters.AddWithValue("@ActualType", Resources.Param_Actual_Type);
                        command.Parameters.AddWithValue("@ProductId", Guid.Empty);

                        connection.Open();
                        foreach (var product in products)
                        {
                            command.Parameters["@ProductId"].Value = product;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Guid GetParentIdByLogin(string login)
        {
            var result = Guid.Empty;
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = Resources.ProcName_GetParentById;
                        command.Parameters.AddWithValue("@login", login);
                        connection.Open();
                        var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            result = (Guid)reader["parentId"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
        public void AddFilial(Guid filialId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = Resources.ProcName_AddFilial;
                        command.Parameters.AddWithValue("@FilialId", filialId);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {                
            }
        }
    }
}
