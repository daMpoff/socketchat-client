using System.Windows;

namespace socketchat
{
    public partial class Register : Window
    {
        public DBCRUD dBCRUD = new DBCRUD();

        public Register()
        {
            InitializeComponent();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginBox.Text.Length > 6 && (passwordBox.Password == repeatpasswordbox.Password))
            {
                if (dBCRUD.Registration(loginBox.Text, passwordBox.Password))
                {
                    this.Close();
                }
            }
            else if (loginBox.Text.Length <= 6)
            {
                MessageBox.Show("Длина логина должна быть больше 6 символов!");
            }
            else if (passwordBox.Password != repeatpasswordbox.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
            }
            else if (loginBox.Text.Length < 8 || repeatpasswordbox.Password.Length < 8)
            {
                MessageBox.Show("Пароль должен быть больше 8 символов!");
            }
        }
        private void loglabel_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }
    }
}