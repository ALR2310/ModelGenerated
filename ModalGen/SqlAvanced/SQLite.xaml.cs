using Microsoft.Win32;
using ModelGen.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Shapes;

namespace ModelGen.SqlAvanced
{
    /// <summary>
    /// Interaction logic for SQLite.xaml
    /// </summary>
    public partial class SQLite : Window
    {
        public SQLite()
        {
            InitializeComponent();

            // Gọi sự kiện TextChanged cho mỗi TextBox
            tbDataSource.TextChanged += TextBox_TextChanged;
            tbVersion.TextChanged += TextBox_TextChanged;
            tbPassword.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tạo chuỗi kết nối từ thông tin đã nhập
            string dataSource = tbDataSource.Text;
            string version = tbVersion.Text;
            string password = tbPassword.Text;

            // Tạo chuỗi kết nối SQL
            string sqlConnectString = $"Data Source={dataSource};Version={version};";

            if (!string.IsNullOrEmpty(password))
            {
                sqlConnectString += $"Password={password};";
            }

            if (cbUseUtf16True.IsSelected)
            {
                sqlConnectString += "UseUTF16Encoding=True;";
            }

            // Gán chuỗi kết nối vào TextBox
            SQlConnectString.Text = sqlConnectString;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            GetConnectString.Instance.ConnectString = SQlConnectString.Text;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Tạo một OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Thiết lập các tùy chọn cho dialog
            openFileDialog.Title = "Chọn tệp tin"; // Tiêu đề của dialog
            openFileDialog.Filter = "Tất cả tệp tin (*.*)|*.*"; // Bộ lọc tệp tin (có thể thay đổi theo loại file bạn muốn chọn)
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Thư mục mặc định

            // Hiển thị dialog và kiểm tra xem người dùng đã chọn tệp tin hay chưa
            if (openFileDialog.ShowDialog() == true)
            {
                // Lấy đường dẫn tệp tin đã chọn
                string selectedFilePath = openFileDialog.FileName;

                tbDataSource.Text = selectedFilePath;
            }
        }
    }
}
