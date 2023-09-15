using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

namespace ClassModel
{
    public class DataAccessLayer
    {
        string ConnectString = @"Data Source=.\sqlexpress;Initial Catalog=school;Integrated Security=True";

        public List<T> ExecuteTypeList<T>(string query, object parameters = null) where T : new()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            var properties = parameters.GetType().GetProperties();
                            foreach (var property in properties)
                            {
                                string paramName = "@" + property.Name;
                                var paramValue = property.GetValue(parameters, null);
                                command.Parameters.AddWithValue(paramName, paramValue ?? DBNull.Value);
                            }
                        }
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable(); adapter.Fill(dataTable);
                        List<T> resultList = new List<T>();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            T obj = new T();
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                var property = typeof(T).GetProperty(col.ColumnName);
                                if (property != null)
                                {
                                    object value = row[col];
                                    if (value != DBNull.Value) { property.SetValue(obj, value); }
                                }
                            }
                            resultList.Add(obj);
                        }
                        return resultList;
                    }
                }
            }
            catch (Exception ex) { throw ex; }
        }

        public int ExecuteNonQuery(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    if (parameters != null)
                    {
                        var properties = parameters.GetType().GetProperties();
                        foreach (var property in properties)
                        {
                            string paramName = "@" + property.Name;
                            var paramValue = property.GetValue(parameters, null);
                            cmd.Parameters.AddWithValue(paramName, paramValue ?? DBNull.Value);
                        }
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { throw ex; }
        }

        public T ExecuteScalar<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectString))
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    if (parameters != null)
                    {
                        var properties = parameters.GetType().GetProperties();
                        foreach (var property in properties)
                        {
                            string paramName = "@" + property.Name;
                            var paramValue = property.GetValue(parameters, null);
                            cmd.Parameters.AddWithValue(paramName, paramValue ?? DBNull.Value);
                        }
                    }
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value) { return (T)Convert.ChangeType(result, typeof(T)); }
                    else { return default(T); }
                }
            }
            catch (Exception ex) { throw ex; }
        }
    }
}
