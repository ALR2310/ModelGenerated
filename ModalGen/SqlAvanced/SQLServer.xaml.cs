using ModelGen.Model;
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
using System.Windows.Shapes;

namespace ModelGen.SqlAvanced
{
    /// <summary>
    /// Interaction logic for SQLServer.xaml
    /// </summary>
    public partial class SQLServer : Window
    {
        public SQLServer()
        {
            InitializeComponent();

            // Gọi sự kiện TextChanged cho mỗi TextBox
            tbDataSource.TextChanged += TextBox_TextChanged;
            tbDataBase.TextChanged += TextBox_TextChanged;
            tbUserID.TextChanged += TextBox_TextChanged;
            tbPassword.TextChanged += TextBox_TextChanged;
            tbTimeout.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Tạo chuỗi kết nối từ thông tin đã nhập
            string dataSource = tbDataSource.Text;
            string database = tbDataBase.Text;
            string userId = tbUserID.Text;
            string password = tbPassword.Text;
            string timeout = tbTimeout.Text;

            // Tạo chuỗi kết nối SQL
            string sqlConnectString;

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(password))
            {
                sqlConnectString = $"Data Source={dataSource};Initial Catalog={database};Integrated Security=True;";
            }
            else
            {
                sqlConnectString = $"Data Source={dataSource};Initial Catalog={database};User ID={userId};Password={password};";
            }

            if (!string.IsNullOrEmpty(timeout))
            {
                sqlConnectString += $"Connection Timeout={timeout};";
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
            DataStore.Instance.SharedData = SQlConnectString.Text;
            this.Close();
        }
    }
}
