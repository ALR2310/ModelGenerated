using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using MessageBox = System.Windows.MessageBox;
using System.Data.Common;
using CheckBox = System.Windows.Controls.CheckBox;

namespace ModalGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private StringBuilder logBuilder = new StringBuilder();
        private Span logBuilder = new Span();
        public MainWindow()
        {
            InitializeComponent();

        }

        private void btnSelectFilePath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            DialogResult result = folderBrowserDialog.ShowDialog();

            logBuilder.Inlines.Add(new Run($"Chọn đường dẫn lưu file\n") { Foreground = Brushes.Black });

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                // Sử dụng selectedFolderPath cho mục đích của bạn
                tbFilePath.Text = selectedFolderPath;
                logBuilder.Inlines.Add(new Run($"Thành công\n") { Foreground = Brushes.Green });
            }
            else
            {
                logBuilder.Inlines.Add(new Run($"Thất bại\n") { Foreground = Brushes.Red });
            }
            tbLogs.Inlines.Add(logBuilder);
        }







        private void tbConnectString_TextChanged(object sender, TextChangedEventArgs e)
        {
            string selectedDatabase = ((ComboBoxItem)cmbDatabaseType.SelectedItem)?.Content.ToString();
            string connectionString = tbConnectString.Text;


            logBuilder.Inlines.Add(new Run($"Chuỗi Kết Nối Loại: {selectedDatabase}\n") { Foreground = Brushes.Black });

            try
            {
                using (DbConnection connection = GetDatabaseConnection(selectedDatabase, connectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                    {
                        logBuilder.Inlines.Add(new Run($"Kết nối đến {selectedDatabase} thành công\n") { Foreground = Brushes.Green });

                        // Truy vấn để lấy danh sách các bảng trong cơ sở dữ liệu
                        DataTable schema = connection.GetSchema("Tables");

                        foreach (DataRow row in schema.Rows)
                        {
                            string tableName = row["TABLE_NAME"].ToString();

                            // Kiểm tra nếu là bảng sysdiagrams thì bỏ qua
                            if (tableName != "sysdiagrams")
                            {
                                CheckBox checkBox = new CheckBox
                                {
                                    Content = tableName,
                                    Tag = tableName // Sử dụng Tag để lưu tên bảng
                                };
                                tableCheckBoxes.Children.Add(checkBox);
                            }
                        }

                        logBuilder.Inlines.Add(new Run($"Lấy thông tin các bảng thành công\n") { Foreground = Brushes.Green });
                    }
                    else
                    {
                        logBuilder.Inlines.Add(new Run($"Kết nối đến {selectedDatabase} thất bại\n") { Foreground = Brushes.Red });
                    }
                }
            }
            catch (Exception ex)
            {
                logBuilder.Inlines.Add(new Run($"Lỗi kết nối: {ex.Message}\n") { Foreground = Brushes.Red });
            }

            tbLogs.Inlines.Add(logBuilder);
        }

        private DbConnection GetDatabaseConnection(string selectedDatabase, string connectionString)
        {
            switch (selectedDatabase)
            {
                case "SQL Server":
                    return new SqlConnection(connectionString);
                // Thêm các case cho các loại cơ sở dữ liệu khác ở đây.
                default:
                    throw new ArgumentException($"Loại cơ sở dữ liệu không được hỗ trợ: {selectedDatabase}");
            }
        }

        private void btnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            tbLogs.Text = string.Empty;
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            // Chuỗi kết nối đến SQL Server
            string connectionString = tbConnectString.Text;
            string outputPath = tbFilePath.Text; // Đường dẫn tới thư mục lưu tệp tin .cs

            if (tbFilePath.Text == string.Empty)
            {
                logBuilder.Inlines.Add(new Run($"Đường dẫn lưu file trống\n") { Foreground = Brushes.Black });
                logBuilder.Inlines.Add(new Run($"Tự động lưu file tại ứng dụng\n") { Foreground = Brushes.Black });
            }

            try
            {
                foreach (CheckBox checkBox in tableCheckBoxes.Children)
                {
                    if (checkBox.IsChecked == true)
                    {
                        string selectedTable = checkBox.Tag.ToString();
                        List<string> columns = new List<string>();

                        // Tạo kết nối SQL
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Truy vấn để lấy thông tin về các cột trong bảng
                            SqlCommand command = new SqlCommand($"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{selectedTable}'", connection);
                            SqlDataReader reader = command.ExecuteReader();
                            logBuilder.Inlines.Add(new Run($"Truy vấn lấy thông tin về các cột trong bảng\n") { Foreground = Brushes.Black });
                            while (reader.Read())
                            {
                                string columnName = reader["COLUMN_NAME"].ToString();
                                string dataType = reader["DATA_TYPE"].ToString();

                                columns.Add($"{columnName} {GetCSharpType(dataType)}");
                            }

                            reader.Close();
                            logBuilder.Inlines.Add(new Run($"Thành công\n") { Foreground = Brushes.Green });
                            tbLogs.Inlines.Add(logBuilder);
                        }

                        logBuilder.Inlines.Add(new Run($"Tạo đoạn mã định nghĩa cho lớp model\n") { Foreground = Brushes.Black });
                        tbLogs.Inlines.Add(logBuilder);
                        // Tạo đoạn mã C# để định nghĩa lớp class model
                        StringBuilder classCode = new StringBuilder();

                        // Thêm các dòng using vào đầu tệp tin
                        classCode.AppendLine("using System;");
                        classCode.AppendLine("using System.Collections.Generic;");
                        classCode.AppendLine("using System.Linq;");
                        classCode.AppendLine("using System.Text;");
                        classCode.AppendLine("using System.Threading.Tasks;");
                        classCode.AppendLine();

                        classCode.AppendLine($"public class {selectedTable}");
                        classCode.AppendLine("{");

                        logBuilder.Inlines.Add(new Run($"Thành công\n") { Foreground = Brushes.Green });
                        tbLogs.Inlines.Add(logBuilder);

                        foreach (var column in columns)
                        {
                            // Sử dụng các trường đã ánh xạ từ kiểu dữ liệu SQL
                            classCode.AppendLine($"    public {column} {{ get; set; }}");
                        }

                        classCode.AppendLine("}");

                        logBuilder.Inlines.Add(new Run($"Chuyển đổi kiểu dữ liệu từ sql sang C#\n") { Foreground = Brushes.Black });
                        logBuilder.Inlines.Add(new Run($"Thành công\n") { Foreground = Brushes.Green });
                        tbLogs.Inlines.Add(logBuilder);

                        // Tạo đường dẫn đầy đủ cho tệp tin .cs
                        string outputFileName = $"{selectedTable}.cs";
                        string outputFileFullPath = System.IO.Path.Combine(outputPath, outputFileName);
                        logBuilder.Inlines.Add(new Run($"Tạo đường dẫn đầy đủ cho tệp tin\n") { Foreground = Brushes.Black });
                        tbLogs.Inlines.Add(logBuilder);

                        // Lưu đoạn mã vào tệp tin .cs
                        System.IO.File.WriteAllText(outputFileFullPath, classCode.ToString());
                        logBuilder.Inlines.Add(new Run($"Thành công\n") { Foreground = Brushes.Green });
                        logBuilder.Inlines.Add(new Run($"Đã lưu tệp tại: {outputFileFullPath}\n") { Foreground = Brushes.Black });
                        tbLogs.Inlines.Add(logBuilder);
                    }
                }
            }
            catch (Exception ex)
            {
                logBuilder.Inlines.Add(new Run($"Lỗi:\n") { Foreground = Brushes.Black });
                logBuilder.Inlines.Add(new Run($"{ex.Message}\n") { Foreground = Brushes.Red });
                tbLogs.Inlines.Add(logBuilder);
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
