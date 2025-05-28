using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证登录逻辑（示例）
            if (ValidateLogin())
            {
                // 创建并显示新窗口
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // 关闭当前窗口
                this.Close();
            }
        }

        private bool ValidateLogin()
        {
            // 实际项目中添加登录验证逻辑
            return !string.IsNullOrEmpty(AccountTextBox.Text) &&
                   !string.IsNullOrEmpty(PasswordBox.Password);
        }
    }
}
