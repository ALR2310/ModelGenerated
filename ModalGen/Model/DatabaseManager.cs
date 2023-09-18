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
using System.Xml.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using Npgsql;
using System.Windows;

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
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex);
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
                            columns.Add($"{GetCSharpType(dataType)} {columnName}");
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex);
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
                case "PostgreSQL":
                    return new NpgsqlConnection(_connectionString);
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
                    return sqlType;
            }
        }

        //Tạo các lớp Model
        public void GeneratedClassModel(string tableName, List<string> columns, string outputPath, string namespaces)
        {
            StringBuilder classCode = new StringBuilder();
            classCode.AppendLine("using System;");
            classCode.AppendLine("using System.Collections.Generic;");
            classCode.AppendLine("using System.Linq;");
            classCode.AppendLine("using System.Text;");
            classCode.AppendLine("using System.Threading.Tasks;");
            classCode.AppendLine();
            classCode.AppendLine($"namespace {namespaces}");
            classCode.AppendLine("{");
            classCode.AppendLine($"    public class {tableName}");
            classCode.AppendLine("    {");

            foreach (var column in columns)
            {
                classCode.AppendLine($"        public {column} {{ get; set; }}");
            }

            classCode.AppendLine("    }");
            classCode.Append("}");

            string outputFileName = $"{tableName}.cs";
            string ClassFilePath = System.IO.Path.Combine(outputPath, "Model");

            if (!System.IO.Directory.Exists(ClassFilePath))
            {
                // Tạo thư mục nếu nó không tồn tại
                System.IO.Directory.CreateDirectory(ClassFilePath);
            }
            string ClassFileFullPath = System.IO.Path.Combine(ClassFilePath, outputFileName);

            System.IO.File.WriteAllText(ClassFileFullPath, classCode.ToString());
        }

        //Tạo tệp Tương tác với bảng
        public void GenerateClassManager(string tableName, List<string> columns, string outputPath, string namespaces)
        {
            if (string.IsNullOrEmpty(namespaces))
            {
                namespaces = "DataAccessLayer";
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            StringBuilder classCode = new StringBuilder();
            classCode.AppendLine("using System;");
            classCode.AppendLine("using System.Collections.Generic;");
            classCode.AppendLine("using System.Linq;");
            classCode.AppendLine("using System.Text;");
            classCode.AppendLine("using System.Web;");
            classCode.AppendLine("using System.Data;");
            classCode.AppendLine("using System.Threading.Tasks;");
            classCode.AppendLine();
            classCode.AppendLine($"namespace {namespaces}");
            classCode.AppendLine("{");
            classCode.AppendLine($"    public class {tableName}Manager");
            classCode.AppendLine("    {");

            // Tạo các phương thức cho hoạt động CRUD dựa trên các cột
            classCode.AppendLine($"        private static readonly DataAccessLayer _dal = new DataAccessLayer();");
            classCode.AppendLine();

            // Tạo phương thức GetAll
            classCode.AppendLine($"        public static List<{tableName}> GetAll()");
            classCode.AppendLine("        {");
            classCode.AppendLine($"            string query = \"SELECT * FROM {tableName}\";");
            classCode.AppendLine($"            return _dal.ExecuteTypeList<{tableName}>(query);");
            classCode.AppendLine("        }");
            classCode.AppendLine();

            // Tạo phương thức GetById
            classCode.AppendLine($"        public static {tableName} GetById(int Id)");
            classCode.AppendLine("        {");
            classCode.AppendLine($"            string query = $\"SELECT * FROM {tableName} WHERE Id = @Id\";");
            classCode.AppendLine($"            return _dal.ExecuteTypeList<{tableName}>(query, new {{ Id }}).FirstOrDefault();");
            classCode.AppendLine("        }");
            classCode.AppendLine();

            // Tạo phương thức Insert
            classCode.AppendLine($"        public static void Insert({string.Join(", ", columns)})");
            classCode.AppendLine("        {");
            classCode.Append($"            string query = $\"INSERT INTO {tableName} VALUES (");
            classCode.Append(string.Join(", ", columns.Select(col => $"@{col.Split(' ')[1]}")));
            classCode.AppendLine($")\";");
            classCode.AppendLine($"            _dal.ExecuteNonQuery(query, new {{{string.Join(", ", columns.Select(col => $"{col.Split(' ')[1]}"))}}});");
            classCode.AppendLine("        }");
            classCode.AppendLine();

            // Tạo phương thức Update
            classCode.AppendLine($"        public static void Update({string.Join(", ", columns)})");
            classCode.AppendLine("        {");
            classCode.Append($"            string query = $\"UPDATE {tableName} SET ");

            for (int i = 1; i < columns.Count; i++) // Tạo các cặp tên cột và tham số để cập nhật
            {
                string columnName = columns[i].Split(' ')[1];
                classCode.Append($"{columnName} = @{columnName}");

                if (i < columns.Count - 1)
                {
                    classCode.Append(", ");
                }
            }

            classCode.AppendLine($" WHERE {columns[0].Split(' ')[1]} = @{columns[0].Split(' ')[1]}\";");
            classCode.AppendLine($"            _dal.ExecuteNonQuery(query, new {{{string.Join(", ", columns.Select(col => $"{col.Split(' ')[1]}"))}}});");
            classCode.AppendLine("        }");
            classCode.AppendLine();

            // Tạo phương thức Delete
            classCode.AppendLine("        public static void Delete(int Id)");
            classCode.AppendLine("        {");
            classCode.AppendLine($"            string query = $\"DELETE FROM {tableName} WHERE Id = @Id\";");
            classCode.AppendLine($"            _dal.ExecuteNonQuery(query, new {{Id}});");
            classCode.AppendLine("        }");
            classCode.AppendLine();

            classCode.AppendLine("    }");
            classCode.AppendLine("}");

            string ManagerFileName = $"{tableName}Manager.cs";
            string ManagerFolderPath = System.IO.Path.Combine(outputPath, "DAL");

            if (!System.IO.Directory.Exists(ManagerFolderPath))
            {
                // Tạo thư mục nếu nó không tồn tại
                System.IO.Directory.CreateDirectory(ManagerFolderPath);
            }
            string DALFileFullPath = System.IO.Path.Combine(ManagerFolderPath, ManagerFileName);

            System.IO.File.WriteAllText(DALFileFullPath, classCode.ToString());
        }

        //Tạo tệp class DAL
        public void GenerateClassDAL(string typleDB, string connectString, string outputPath, string namespaces)
        {
            string symbol = "{";
            StringBuilder cls = new StringBuilder();

            string SqlConnection = "SqlConnection", SqlDataAdapter = "SqlDataAdapter", SqlCommand = "SqlCommand";

            if (typleDB == "MySQL") { SqlConnection = "MySqlConnection"; SqlDataAdapter = "MySqlDataAdapter"; SqlCommand = "MySqlCommand"; }
            else if (typleDB == "SQLite") { SqlConnection = "SQLiteConnection"; SqlDataAdapter = "MySqlDataAdapter"; SqlCommand = "SQLiteCommand"; }

            cls.AppendLine("using System;");
            cls.AppendLine("using System.Collections.Generic;");
            cls.AppendLine("using System.Data.SqlClient;");
            cls.AppendLine("using System.Data;");
            cls.AppendLine("using System.Linq;");
            cls.AppendLine();
            cls.AppendLine($"namespace {namespaces} {symbol}");
            cls.AppendLine($"    public class DataAccessLayer {symbol} string ConnectString = @\"{connectString}\";");
            cls.AppendLine();
            cls.AppendLine("        public List<T> ExecuteTypeList<T>(string query, object parameters = null) where T : new() {");
            cls.AppendLine("            try { ");
            cls.AppendLine($"                using ({SqlConnection} connection = new {SqlConnection}(ConnectString))");
            cls.AppendLine("                {connection.Open();");
            cls.AppendLine($"                using ({SqlCommand} command = new {SqlCommand}(query, connection))");
            cls.AppendLine("                    { if (parameters != null) {var properties = parameters.GetType().GetProperties();");
            cls.AppendLine("                        foreach (var property in properties) {string paramName = \"@\" + property.Name;");
            cls.AppendLine("                            var paramValue = property.GetValue(parameters, null);");
            cls.AppendLine("                            command.Parameters.AddWithValue(paramName, paramValue ?? DBNull.Value);}}");
            cls.AppendLine($"                    {SqlDataAdapter} adapter = new {SqlDataAdapter}(command);");
            cls.AppendLine("                    DataTable dataTable = new DataTable();adapter.Fill(dataTable);");
            cls.AppendLine("                    List<T> resultList = new List<T>();");
            cls.AppendLine("                    foreach (DataRow row in dataTable.Rows) {T obj = new T();");
            cls.AppendLine("                        foreach (DataColumn col in dataTable.Columns) {var property = typeof(T).GetProperty(col.ColumnName);");
            cls.AppendLine("                            if (property != null) {object value = row[col];");
            cls.AppendLine("                                if (value != DBNull.Value) {property.SetValue(obj, value);}}}resultList.Add(obj);}");
            cls.AppendLine("                    return resultList;}}}catch (Exception ex) { throw ex; } }");
            cls.AppendLine();
            cls.AppendLine("        public int ExecuteNonQuery(string query, object parameters = null) {");
            cls.AppendLine("            try { ");
            cls.AppendLine($"                using ({SqlConnection} connection = new SqlConnection({SqlConnection})) ");
            cls.AppendLine("                { connection.Open(); ");
            cls.AppendLine($"                {SqlCommand} cmd = new {SqlCommand}(query, connection);");
            cls.AppendLine("                if (parameters != null) {var properties = parameters.GetType().GetProperties();");
            cls.AppendLine("                    foreach (var property in properties) {string paramName = \"@\" + property.Name;");
            cls.AppendLine("                        var paramValue = property.GetValue(parameters, null);");
            cls.AppendLine("                        cmd.Parameters.AddWithValue(paramName, paramValue ?? DBNull.Value);}}");
            cls.AppendLine("                    return cmd.ExecuteNonQuery();}}catch (Exception ex){throw ex;}}");
            cls.AppendLine();
            cls.AppendLine("        public T ExecuteScalar<T>(string query, object parameters = null){");
            cls.AppendLine("            try{ ");
            cls.AppendLine($"                using ({SqlConnection} connection = new {SqlConnection}(ConnectString))");
            cls.AppendLine("                { connection.Open();");
            cls.AppendLine($"                {SqlCommand} cmd = new {SqlCommand}(query, connection);");
            cls.AppendLine("                if (parameters != null){ var properties = parameters.GetType().GetProperties();");
            cls.AppendLine("                    foreach (var property in properties){ string paramName = \"@\" + property.Name;");
            cls.AppendLine("                        var paramValue = property.GetValue(parameters, null);");
            cls.AppendLine("                        cmd.Parameters.AddWithValue(paramName, paramValue ?? DBNull.Value);}}");
            cls.AppendLine("                    object result = cmd.ExecuteScalar();");
            cls.AppendLine("                    if (result != null && result != DBNull.Value){return (T)Convert.ChangeType(result, typeof(T));}");
            cls.AppendLine("                    else{return default(T);}}}catch (Exception ex){throw ex;}}}}");

            string DALFolderPath = System.IO.Path.Combine(outputPath, "DAL");
            if (!System.IO.Directory.Exists(DALFolderPath))
            {
                // Tạo thư mục nếu nó không tồn tại
                System.IO.Directory.CreateDirectory(DALFolderPath);
            }
            string DALFileFullPath = System.IO.Path.Combine(DALFolderPath, "DataAccessLayer.cs");

            System.IO.File.WriteAllText(DALFileFullPath, cls.ToString());
        }
    }
}
