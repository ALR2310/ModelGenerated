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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ModelGen.SqlAvanced
{
    /// <summary>
    /// Interaction logic for PostgreSQL.xaml
    /// </summary>
    public partial class PostgreSQL : Window
    {
        public PostgreSQL()
        {
            InitializeComponent();

            // Gọi sự kiện TextChanged cho mỗi TextBox
            tbDataSource.TextChanged += TextBox_TextChanged;
            tbDataBase.TextChanged += TextBox_TextChanged;
            tbUserID.TextChanged += TextBox_TextChanged;
            tbPassword.TextChanged += TextBox_TextChanged;
            tbPort.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tạo chuỗi kết nối từ thông tin đã nhập
            string dataSource = tbDataSource.Text;
            string database = tbDataBase.Text;
            string userId = tbUserID.Text;
            string password = tbPassword.Text;
            string port = tbPort.Text;

            // Tạo chuỗi kết nối SQL
            string sqlConnectString = $"Host={dataSource};Port={port};Username={userId};Password={password};Database={database}";

            // Gán chuỗi kết nối vào TextBox
            SQlConnectString.Text = sqlConnectString;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            GetConnectString.Instance.ConnectString = SQlConnectString.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
