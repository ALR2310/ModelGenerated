using ModelGen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MySQL.xaml
    /// </summary>
    public partial class MySQL : Window
    {
        public MySQL()
        {
            InitializeComponent();

            // Gọi sự kiện TextChanged cho mỗi TextBox
            tbDataSource.TextChanged += TextBox_TextChanged;
            tbDataBase.TextChanged += TextBox_TextChanged;
            tbUserID.TextChanged += TextBox_TextChanged;
            tbPassword.TextChanged += TextBox_TextChanged;
            tbTcpPort.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tạo chuỗi kết nối từ thông tin đã nhập
            string dataSource = tbDataSource.Text;
            string database = tbDataBase.Text;
            string userId = tbUserID.Text;
            string password = tbPassword.Text;
            string port = tbTcpPort.Text;

            // Tạo chuỗi kết nối SQL
            string sqlConnectString = $"Server={dataSource};Database={database};Uid={userId};Pwd={password};";

            if (!string.IsNullOrEmpty(port))
            {
                sqlConnectString += $"Port={port};";
            }

            if (cbEncryptTrue.IsSelected)
            {
                sqlConnectString += "Encrypt=True;";
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


    }
}
