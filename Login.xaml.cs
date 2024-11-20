using System.Windows;

namespace socketchat
{
    public partial class Login : Window
    {
        public DBCRUD dBCRUD = new DBCRUD();
        public Login()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (dBCRUD.Login(loginBox.Text, passwordBox.Password))
            {
                this.Close();
            }
        }
        private void loglabel_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Register register = new Register();
            register.Show();
            this.Close();
        }
    }
}
