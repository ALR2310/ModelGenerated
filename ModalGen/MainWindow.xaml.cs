using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using CheckBox = System.Windows.Controls.CheckBox;
using MySql.Data.MySqlClient;
using ModelGen.SqlAvanced;
using ModelGen.Model;
using System.Data.SQLite;

namespace ModalGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private StringBuilder logBuilder = new StringBuilder();
        private Span logBuilder = new Span();

        //sql server = Data Source=.\sqlexpress;Initial Catalog=QuanLyRaVaoCty;Integrated Security=True
        //mysql = Server=localhost;Database=quanlysinhvien;User=root;Password=123456;
        //sqlite = Data Source=D:\LeThanhAn\SQLite\QLCT;Version=3;
        public MainWindow()
        {
            InitializeComponent();

            GetConnectString.Instance.OnDataChanged += (data) =>
            {
                tbConnectString.Text = data;
            };

            tbConnectString.Text = @"Data Source=.\sqlexpress;Initial Catalog=school;Integrated Security=True";
            tbFilePath.Text = @"C:\Users\ALRIP\Desktop\TEST";
        }

        //Xử lý chọn đường dẫn file sẽ xuất hiện
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

        //Clear nhật kí
        private void btnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            logBuilder.Inlines.Clear();
            tbLogs.Inlines.Add(logBuilder);
        }

        //Mở cửa sổ tuỳ chỉnh sql string
        private void btnOpenSQLConnect_Click(object sender, RoutedEventArgs e)
        {
            string selectedDatabase = ((ComboBoxItem)cmbDatabaseType.SelectedItem)?.Content.ToString();

            switch (selectedDatabase)
            {
                case "SQL Server":
                    SQLServer msql = new SQLServer();
                    msql.ShowDialog();
                    break;
                case "MySQL":
                    MySQL mysql = new MySQL();
                    mysql.ShowDialog();
                    break;
                case "SQLite":
                    SQLite sqlite = new SQLite();
                    sqlite.ShowDialog();
                    break;
            }
        }

        // Nút kiểm tra kết nối
        private void btnCheckConnect_Click(object sender, RoutedEventArgs e)
        {
            ClearTableCheckBoxes();

            string selectedDatabase = ((ComboBoxItem)cmbDatabaseType.SelectedItem)?.Content.ToString();
            string connectionString = tbConnectString.Text;

            logBuilder.Inlines.Add(new Run($"Chuỗi Kết Nối Loại: {selectedDatabase}\n") { Foreground = Brushes.Black });

            DatabaseManager dbManager = new DatabaseManager(selectedDatabase, connectionString);
            DataTable schema;

            bool isConnected = dbManager.TestConnection(out schema);

            if (isConnected)
            {
                foreach (DataRow row in schema.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    if (tableName != "sysdiagrams")
                    {
                        CheckBox checkBox = new CheckBox
                        {
                            Content = tableName,
                            Tag = tableName, // Sử dụng Tag để lưu tên bảng
                            IsChecked = true // Chọn mặc định
                        };
                        tableCheckBoxes.Children.Add(checkBox);
                    }
                }

                logBuilder.Inlines.Add(new Run($"Kết nối đến {selectedDatabase} thành công\n") { Foreground = Brushes.Green });
                logBuilder.Inlines.Add(new Run($"Lấy thông tin các bảng thành công\n") { Foreground = Brushes.Green });
            }
            else
            {
                logBuilder.Inlines.Add(new Run($"Kết nối đến {selectedDatabase} thất bại\n") { Foreground = Brushes.Red });
            }

            tbLogs.Inlines.Add(logBuilder);
        }


        // Nút tạo lớp model
        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = tbConnectString.Text; // Chuỗi kết nối
            string outputPath = tbFilePath.Text; // Đường dẫn lưu tệp tin .cs
            string namespaces = tbNamespace.Text; // Tên không gian tệp

            if (string.IsNullOrEmpty(namespaces))
            {
                namespaces = "ClassModel";
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = AppDomain.CurrentDomain.BaseDirectory; // Lưu file tại thư mục ứng dụng nếu đường dẫn trống
            }

            try
            {
                DatabaseManager dbManager = new DatabaseManager((string)((ComboBoxItem)cmbDatabaseType.SelectedItem)?.Content, connectionString);

                if (cboxClassDAl.IsChecked == true)
                {
                    //Tạo lớp DAL với cấu trúc của từng database
                    switch ((string)((ComboBoxItem)cmbDatabaseType.SelectedItem)?.Content)
                    {
                        case "SQL Server":
                            dbManager.GenerateClassDAL("SQL Server", connectionString, outputPath, namespaces);
                            break;
                        case "MySQL":
                            dbManager.GenerateClassDAL("MySQL", connectionString, outputPath, namespaces);
                            break;
                        case "SQLite":
                            dbManager.GenerateClassDAL("SQLite", connectionString, outputPath, namespaces);
                            break;
                        default:
                            logBuilder.Inlines.Add(new Run($"Tạo tệp thất bại. chưa hỗ trợ loại SQL này\n") { Foreground = Brushes.Red });
                            break;
                    }
                    string managerFullFilePath = System.IO.Path.Combine(outputPath + "\\DAL", "DataAccessLayer.cs");
                    logBuilder.Inlines.Add(new Run($"Tạo Tệp DataAccessLayer.cs thành công\n") { Foreground = Brushes.Green });
                    logBuilder.Inlines.Add(new Run($"Đã lưu tệp tại: {managerFullFilePath}\n") { Foreground = Brushes.Black });
                }

                foreach (CheckBox checkBox in tableCheckBoxes.Children)
                {
                    if (checkBox.IsChecked == true)
                    {
                        string selectedTable = checkBox.Tag.ToString();
                        List<string> columns = dbManager.GetTableColumns(selectedTable);

                        // Tạo lớp Model
                        dbManager.GeneratedClassModel(selectedTable, columns, outputPath, namespaces);

                        // Tạo lớp Manager cho các model
                        if (cboxClassModelManager.IsChecked == true)
                        {
                            dbManager.GenerateClassManager(selectedTable, columns, outputPath, namespaces);

                            string managerFileName = $"{selectedTable}Manager.cs";
                            string managerFullFilePath = System.IO.Path.Combine(outputPath + "\\DAL", managerFileName);
                            logBuilder.Inlines.Add(new Run($"Tạo Tệp {selectedTable}Manager.cs thành công\n") { Foreground = Brushes.Green });
                            logBuilder.Inlines.Add(new Run($"Đã lưu tệp tại: {managerFullFilePath}\n") { Foreground = Brushes.Black });
                        }

                        string outputFileName = $"{selectedTable}.cs";
                        string outputFileFullPath = System.IO.Path.Combine(outputPath + "\\Model", outputFileName);

                        logBuilder.Inlines.Add(new Run($"Tạo Tệp {selectedTable}.cs thành công\n") { Foreground = Brushes.Green });
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

        //Xoá danh sách bảng trong checkbox
        private void ClearTableCheckBoxes()
        {
            tableCheckBoxes.Children.Clear();
        }

    }
}
