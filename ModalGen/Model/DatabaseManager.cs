using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace ModelGen.Model
{
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private readonly string _databaseType;

        public DatabaseManager(string databaseType, string connectionString)
        {
            _databaseType = databaseType;
            _connectionString = connectionString;
        }

        public bool TestConnection(out DataTable schema)
        {
            schema = null;

            try
            {
                using (DbConnection connection = GetDatabaseConnection())
                {
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        schema = connection.GetSchema("Tables");
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }

            return false;
        }

        public List<string> GetTableColumns(string tableName)
        {
            List<string> columns = new List<string>();

            try
            {
                using (DbConnection connection = GetDatabaseConnection())
                {
                    connection.Open();

                    if (connection.State == ConnectionState.Open)
                    {
                        DbCommand command = connection.CreateCommand();
                        command.CommandText = $"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
                        DbDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            string columnName = reader["COLUMN_NAME"].ToString();
                            string dataType = reader["DATA_TYPE"].ToString();
                            columns.Add($"{columnName} {GetCSharpType(dataType)}");
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }

            return columns;
        }

        private DbConnection GetDatabaseConnection()
        {
            switch (_databaseType)
            {
                case "SQL Server":
                    return new SqlConnection(_connectionString);
                case "MySQL":
                    return new MySqlConnection(_connectionString);
                case "SQLite":
                    return new SQLiteConnection(_connectionString);
                default:
                    throw new ArgumentException($"Loại cơ sở dữ liệu không được hỗ trợ {_databaseType}");
            }
        }

        //Danh sách chuyển đổi kiểu sql sang kiểu C#
        private string GetCSharpType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "int":
                    return "int";
                case "bigint":
                    return "long";
                case "smallint":
                    return "short";
                case "tinyint":
                    return "byte";
                case "float":
                    return "float";
                case "real":
                    return "double";
                case "decimal":
                    return "decimal";
                case "money":
                case "smallmoney":
                    return "decimal";
                case "date":
                case "time":
                case "datetime":
                case "smalldatetime":
                    return "DateTime";
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    return "string";
                case "bit":
                    return "bool";
                default:
                    return "object";
            }
        }
    }
}
