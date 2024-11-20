using socketchat.Models;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace socketchat
{
    public partial class MainWindow : Window
    {
        private bool focusIP, focusPort = false;//переменные фокуса на полях ввода текста
        private bool firstChangeClick = true;
        ChatClient chatClient = new ChatClient();
        DBCRUD dBCRUD = new DBCRUD();//создаём экземпляр клиента, который мы будем юзать из главного окна
        public MainWindow()
        {
            InitializeComponent();
            messageList.ItemsSource = ChatClient.composite;//коллекция источник сообщений
        }
        #region Прогрузка данных в профиль
        private void ProfileLabel_Loaded(object sender, RoutedEventArgs e)
        {
            ProfileLabel.Text = $"Логин: {UserData.UserLogin}";
        }
        private void imgprofile_Loaded(object sender, RoutedEventArgs e)
        {
            ProfileImage.ImageSource = new BitmapImage(new Uri(UserData.ProfileImgPath, UriKind.Absolute));
        }
        private void UserSurnameBox_Loaded(object sender, RoutedEventArgs e)
        {
            UserSurnameBox.Text = UserData.UserSurname;
        }
        private void UserNameBox_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameBox.Text = UserData.UserName;
        }
        private void UserPatronumBox_Loaded(object sender, RoutedEventArgs e)
        {
            UserPatronumBox.Text = UserData.UserPatronum;
        }
        private void DescriptionBox_Loaded(object sender, RoutedEventArgs e)
        {
            DescriptionBox.Text = UserData.UserDescription;
        }
        private void DateOfBirthBox_Loaded(object sender, RoutedEventArgs e)
        {
            DateOfBirthBox.SelectedDate = UserData.DateOfBirth;
        }
        #endregion

        #region Кнопка смены пользователя
        private void ChangeUserButton_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }
        #endregion

        #region Кнопка выхода из приложения
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Смена изображения профиля
        private void imgprofile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WorkWithImageApi.ChangeProfileImageAsync(ProfileImage);
        }
        #endregion

        #region Фокус на ввод данных сервера
        private void PortAdressBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!focusPort)
            {
                PortAdressBox.Text = "";
                focusPort = true;
            }
        }
        private void IpAdressBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!focusIP)
            {
                IpAdressBox.Text = "";
                focusIP = true;
            }
        }
        #endregion

        #region Подлючение и отключение от сервера

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!chatClient.IsConnect && await chatClient.Connect(IpAdressBox.Text, PortAdressBox.Text))
            {
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                DisconnectButton.Visibility = Visibility.Visible;
                SendFileButton.Visibility = Visibility.Visible;
                SendFileButton.IsEnabled = true;
                SendMessagetButton.Visibility = Visibility.Visible;
                sendMessageTextBox.IsEnabled = true;
            }
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatClient.IsConnect)
            {
                chatClient.Disconnect();
                chatClient.IsConnect = false;
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
                DisconnectButton.Visibility = Visibility.Hidden;
                SendFileButton.Visibility = Visibility.Hidden;
                SendFileButton.IsEnabled = false;
                sendMessageTextBox.IsEnabled = false;
                SendMessagetButton.Visibility = Visibility.Hidden;
            }
        }
        #endregion

        #region Удаление аккаунта
        private void DeleteCurrentUser_Click(object sender, RoutedEventArgs e)
        {
            if (DBCRUD.DeleteAccountFunction())
            {
                Close();
            }
        }
        #endregion

        #region Разблокировка кнопки отправки сообщений и файлов
        private void sendMessageTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(sendMessageTextBox.Text) && chatClient.IsConnect)
            {
                SendMessagetButton.IsEnabled = true;
            }
            else
            {
                SendMessagetButton.IsEnabled = false;
            }
        }
        #endregion

        #region Отправка сообщения
        private void SendMessagetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sendMessageTextBox.Text != string.Empty)//проверяем, то что пользователь не отправил пустую строку
            {
                chatClient.TrySendTextMessage(sendMessageTextBox.Text);
                sendMessageTextBox.Clear();//чистим блок ввода сообщений
            }
        }
        #endregion

        #region Отправка файла либо изображения
        private void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatClient.IsConnect)
            {
                chatClient.TrySendFile();
            }
        }
        #endregion

        #region Скачивание файла
        private void DownloadFileButton_Click(object sender, RoutedEventArgs e)
        {
            ChatClient.DownloadFile(sender);
        }
        #endregion

        #region Закрытие потоков при выходе из приложения
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            chatClient.Disconnect();
        }

        #endregion

        #region Смена данных пользователя
        private void ChangeUserdataButton_Click(object sender, RoutedEventArgs e)
        {
            if (firstChangeClick)
            {
                firstChangeClick = false;
                ChangeUserDataIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Check;
                UserSurnameBox.IsReadOnly = false;
                UserNameBox.IsReadOnly = false;
                UserPatronumBox.IsReadOnly = false;
                DescriptionBox.IsReadOnly = false;
                DateOfBirthBox.IsEnabled = true;
            }
            else
            {
                firstChangeClick = true;
                ChangeUserDataIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pencil;
                UserSurnameBox.IsReadOnly = true;
                UserNameBox.IsReadOnly = true;
                UserPatronumBox.IsReadOnly = true;
                DescriptionBox.IsReadOnly = true;
                DateOfBirthBox.IsEnabled = false;
                dBCRUD.ChangeUserData(UserSurnameBox.Text, UserNameBox.Text, UserPatronumBox.Text, DescriptionBox.Text, DateOfBirthBox.SelectedDate);
            }
        }
        #endregion

        #region Контекстное меню
        private void SearchUser(object sender, RoutedEventArgs e)
        {
            chatClient.SearchSwitch(messageList, userList, MainTabControl);
        }
        private void searchUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(searchUserBlock.Text))
            {
                userList.ItemsSource = DBCRUD.SearchUser(searchUserBlock.Text);
            }
            else
            {
                MessageBox.Show("Вы должны ввести значение для поиска!");
            }
        }

        private void DisplayUser(object sender, RoutedEventArgs e)
        {
            User user = (User)userList.SelectedItem;
            UserSearchLoginBox.Text = $"Логин: {user.UserLogin}";
            UserSearchNameBox.Text = $"Имя: {user.UserName}";
            UserSearchPatronumBox.Text = $"Отчество: {user.UserPatronum}";
            UserSearchSurnameBox.Text = $"Фамилия: {user.UserSurname}";
            UserSearchDateOfBirthBox.Text = $"Дата рождения: {user.DateOfBirth.ToShortDateString()}";
            UserSearchDescriptionBox.Text = $"Описание: {user.UserDescription}";
            UserProfileImage.ImageSource = new BitmapImage(new Uri(user.ProfileImgPath, UriKind.Absolute));
        }
        private void BrowserOpen(object sender, RoutedEventArgs e)
        {
            chatClient.OpenImageInBrowser(messageList);
        }
        #endregion
    }
}