using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace socketchat.Models
{
    public class ChatClient
    {
        public bool IsConnect { get; set; }
        private TcpClient client = null;
        private StreamReader reader = null;
        private StreamWriter writer = null;
        private Dispatcher dispatcher;

        public static ObservableCollection<Message> messages = new ObservableCollection<Message>();
        public static ObservableCollection<ImageMessage> images = new ObservableCollection<ImageMessage>();
        public static ObservableCollection<FileMessage> files = new ObservableCollection<FileMessage>();
        public static CompositeCollection composite = new CompositeCollection();

        public ChatClient()
        {
            composite.Add(new CollectionContainer() { Collection = messages });
            composite.Add(new CollectionContainer() { Collection = images });
            composite.Add(new CollectionContainer() { Collection = files });
        }
        public void SearchSwitch(ListBox messageList, ListBox userList, TabControl mainTab)
        {
            if (messageList.SelectedItem != null)
            {
                switch (messageList.SelectedItem.ToString())
                {
                    case "socketchat.Message": SearchUserByMessage(messageList.SelectedItem, userList, mainTab); break;
                    case "socketchat.ImageMessage": SearchUserByImageMessage(messageList.SelectedItem, userList, mainTab); break;
                    case "socketchat.FileMessage": SearchUserByFileMessage(messageList.SelectedItem, userList, mainTab); break;
                    default: break;
                }
            }
        }
        private void SearchUserByMessage(object selectItem, ListBox userList, TabControl maintab)
        {
            Message tempMessage = (Message)selectItem;
            userList.ItemsSource = DBCRUD.SearchUser(tempMessage.UserId.ToString());
            maintab.SelectedIndex = 2;
        }
        private void SearchUserByImageMessage(object selectItem, ListBox userList, TabControl maintab)
        {
            ImageMessage tempMessage = (ImageMessage)selectItem;
            userList.ItemsSource = DBCRUD.SearchUser(tempMessage.UserId.ToString());
            maintab.SelectedIndex = 2;
        }
        private void SearchUserByFileMessage(object selectItem, ListBox userList, TabControl maintab)
        {
            FileMessage tempMessage = (FileMessage)selectItem;
            userList.ItemsSource = DBCRUD.SearchUser(tempMessage.UserId.ToString());
            maintab.SelectedIndex = 2;
        }
        public async Task<bool> Connect(string Ip, string StrPort)
        {
            dispatcher = Application.Current.Dispatcher;
            if (!IsConnect)
            {
                try
                {
                    client = new TcpClient();
                    string IP = Ip;
                    int.TryParse(StrPort, out int Port);
                    await client.ConnectAsync(IP, Port);
                    reader = new StreamReader(client.GetStream());
                    writer = new StreamWriter(client.GetStream());
                    await writer.WriteLineAsync($"{UserData.ProfileImgPath};{UserData.UserLogin};{UserData.UserId}");
                    await writer.FlushAsync();
                    IsConnect = true;
                    MessageBox.Show("Успешное подлючение");
                    Task.Run(() => ReceiveMessages());
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Возникла ошибка!\n{ex.Message}");
                    return false;
                }
            }
            return false;
        }
        private void ClearMessages()
        {
            messages.Clear();
            images.Clear();
            files.Clear();
            composite.Clear();
        }
        public void Disconnect()
        {
            if (IsConnect)
            {
                try
                {
                    IsConnect = false;
                    writer?.Close();
                    reader?.Close();
                    client?.Close();
                    ClearMessages();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Возникла ошибка при закрытии!\n{ex.Message}");
                }
            }
        }
        public static void DownloadFile(object sender)
        {
            Button button = (Button)sender;
            FileMessage fm = (FileMessage)button.DataContext;
            try
            {
                WinForms.FolderBrowserDialog folderBrowserDialog = new WinForms.FolderBrowserDialog();
                folderBrowserDialog.ShowDialog();
                if (!string.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
                {
                    try
                    {
                        string filepath = folderBrowserDialog.SelectedPath;
                        string fullpath = Path.Combine(filepath, Path.GetFileName(fm.FileName));
                        byte[] bytes = Convert.FromBase64String(fm.FileContent);
                        File.WriteAllBytes(fullpath, bytes);
                        MessageBox.Show("Файл был успешно сохранён");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Возникла ошибка!\n{ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Вы должны выбрать директорию для сохранения файла");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при попытке сохранить файл!\n{ex.Message}");
            }
        }
        private async Task ReceiveMessages()
        {
            try
            {
                while (IsConnect)
                {
                    string message = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(message))
                    {
                        continue;
                    }
                    dispatcher.Invoke(new Action(() => PrintMessageSwitch(message)));
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Вы были успешно отключены!");
            }
        }
        private void PrintMessageSwitch(string message)
        {
            List<string> Message = message.Split(';').ToList();
            string swap = Message[0];
            switch (swap)
            {
                case "000": PrintHelloMessage(message); break;
                case "001": PrintExitMessage(message); break;
                case "010": PrintTextMessage(message); break;
                case "011": PrintFileOrImageSwitch(message); break;
            }
        }
        private void PrintFileOrImageSwitch(string message)
        {
            List<string> Message = message.Split(';').ToList();
            string swap = Message[1];
            switch (swap)
            {
                case "000": PrintImageMessage(message); break;
                case "001": PrintFileMessage(message); break;
                default: break;
            }
        }
        private void PrintFileMessage(string fileMessage)
        {
            List<string> Message = fileMessage.Split(';').ToList();
            if (FileMessage.GetSha256File(Message[2], Message[3]) == Message[4])
            {
                composite.Add(new FileMessage(DateTime.Now, Path.GetFileName(Message[2]), (long)Message[3].Length, Message[3], Message[5], Message[6], ulong.Parse(Message[7])));
            }
        }
        private void PrintImageMessage(string ImageMessage)
        {
            List<string> Message = ImageMessage.Split(';').ToList();
            composite.Add(new ImageMessage(DateTime.Now, Message[2], Message[3], Message[4], ulong.Parse(Message[5])));
        }
        private void PrintTextMessage(string textmessage)
        {
            List<string> Message = textmessage.Split(';').ToList();
            composite.Add(new Message(DateTime.Now, Message[1], Message[2], Message[3], ulong.Parse(Message[4])));
        }
        private void PrintExitMessage(string exitmessage)
        {
            List<string> Message = exitmessage.Split(';').ToList();
            composite.Add(new Message(DateTime.Now, $"{Message[2]} покинул чат", Message[1], Message[2], ulong.Parse(Message[3])));
        }
        private void PrintHelloMessage(string hellomessage)
        {
            List<string> Message = hellomessage.Split(';').ToList();
            composite.Add(new Message(DateTime.Now, $"{Message[2]} вошёл в чат", Message[1], Message[2], ulong.Parse(Message[3])));
        }
        private async Task SendMessageAsync(string message)
        {
            try
            {
                await writer.WriteLineAsync(message);
                await writer.FlushAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка!\n{ex.Message}");
            }
        }
        public void TrySendTextMessage(string message)
        {
            TrySendMessage($"010;{message.Replace(";", "")};{UserData.ProfileImgPath};{UserData.UserLogin};{UserData.UserId}");
        }
        public void TrySendMessage(string message)
        {
            Task.Run(() => SendMessageAsync(message));
        }
        private static async Task<string> GetBytesFile(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                byte[] buffer = null;
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int)fs.Length);
                string data = Convert.ToBase64String(buffer);
                return $"{data};{FileMessage.GetSha256File(filePath, data)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при чтении файла\n{ex.Message}");
                return null;
            }
            finally
            {
                fs.Close();
            }
        }
        public async void TrySendFile()
        {
            if (IsConnect == true)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == true)
                {
                    string filePath = ofd.FileName;
                    if (Path.GetExtension(filePath).Contains(".jpeg") || Path.GetExtension(filePath).Contains(".jpg") || Path.GetExtension(filePath).Contains(".png"))
                    {
                        string fileurl = await WorkWithImageApi.GetUrlImageAsync(filePath);
                        if (!string.IsNullOrEmpty(fileurl))
                        {
                            Task.Run(() => SendMessageAsync($"011;000;{fileurl};{UserData.ProfileImgPath};{UserData.UserLogin};{UserData.UserId}"));
                        }
                    }
                    else
                    {
                        Task.Run(async () => SendMessageAsync($"011;001;{filePath};{await GetBytesFile(filePath)};{UserData.ProfileImgPath};{UserData.UserLogin};{UserData.UserId}"));
                    }
                }
            }
            else
            {
                MessageBox.Show("Ошибка\nВы должны быть подключены к серверу!");
            }
        }
        internal void OpenImageInBrowser(ListBox messageList)
        {
            ImageMessage image = (ImageMessage)messageList.SelectedItem;
            Process.Start(new ProcessStartInfo(image.ImgContentPath) { UseShellExecute = true });
        }
    }
}