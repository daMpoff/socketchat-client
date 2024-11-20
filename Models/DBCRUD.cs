using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace socketchat
{
    public class DBCRUD
    {
        static readonly string connStr = $"server={SERVER} database={DB} user={USER} password={PASSWORD}";
        static readonly MySqlConnection conn = new MySqlConnection(connStr);

        #region Шифрование пароля

        private string GetCryptPassword(string login, string RawPassword)
        {
            string salt = login + RawPassword;
            SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(salt);
            byte[] hash = sha256Hash.ComputeHash(bytes);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }
            return builder.ToString();
        }
        #endregion

        #region Регистрация

        public bool Registration(string Login, string UserPassword)
        {
            DateTime RegisterTime = DateTime.Now;
            string ImgPath = @"https://i.imgur.com//mnXOlcq.png";
            try
            {
                conn.Open();
                string GetIdUser = $"SELECT LAST_INSERT_ID();";
                string sql = $"INSERT INTO petrachenkovdb.account (login,hashpassword) VALUES (?user,?password);";
                string sql2 = $"INSERT INTO petrachenkovdb.userdata (iduserdata,name,surname,patronum,description,imageurl) VALUES (?userid,?name,?surname,?patronum,?description,?imageurl)";
                string sql3 = $"INSERT INTO petrachenkovdb.usertime (idusertime,registrationtime,lastlogintime) VALUES (?userid,?regtime,?lastlogtime)";

                //регистрация пользователя в базу данных
                try
                {
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    command.Parameters.Add("?user", MySqlDbType.VarChar).Value = $"{Login}";
                    command.Parameters.Add("?password", MySqlDbType.VarChar).Value = $"{GetCryptPassword(Login, UserPassword)}";
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                //получение id пользователя
                try
                {
                    MySqlCommand command = new MySqlCommand(GetIdUser, conn);
                    UserData.UserId = Convert.ToUInt64(command.ExecuteScalar());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Возникла ошибка при получении id пользователя!\n{ex.Message}");
                }
                //инициализация хранения данных пользователя после инициализации
                try
                {
                    MySqlCommand userdatacommand = new MySqlCommand(sql2, conn);
                    userdatacommand.Parameters.Add("?userid", MySqlDbType.UInt64).Value = $"{UserData.UserId}";
                    userdatacommand.Parameters.Add("?name", MySqlDbType.VarChar).Value = string.Empty;
                    userdatacommand.Parameters.Add("?surname", MySqlDbType.VarChar).Value = string.Empty;
                    userdatacommand.Parameters.Add("?patronum", MySqlDbType.VarChar).Value = string.Empty;
                    userdatacommand.Parameters.Add("?description", MySqlDbType.VarChar).Value = string.Empty;
                    userdatacommand.Parameters.Add("?imageurl", MySqlDbType.VarChar).Value = ImgPath;
                    userdatacommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Возникла ошибка при инициализации пользовательских данных!\n{e.Message}");
                }
                try
                {
                    MySqlCommand usertimecommand = new MySqlCommand(sql3, conn);
                    usertimecommand.Parameters.Add("?userid", MySqlDbType.UInt64).Value = UserData.UserId;
                    usertimecommand.Parameters.Add("?regtime", MySqlDbType.DateTime).Value = RegisterTime;
                    usertimecommand.Parameters.Add("?lastlogtime", MySqlDbType.DateTime).Value = RegisterTime;
                    usertimecommand.ExecuteNonQuery();
                }
                catch
                {
                    MessageBox.Show("Произошла ошибка при попытке инициализации полей времени пользователя");
                }
                UserData.UserLogin = Login;
                UserData.LastLoginTime = RegisterTime;
                UserData.ProfileImgPath = ImgPath;

                MainWindow mainWindow = new MainWindow();
                MessageBox.Show("Успешная регистрация!");
                mainWindow.Show();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Произошла ошибка!\n{e.Message}");
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region Вход

        public bool Login(string Login, string UserPassword)
        {
            string updateTime = $"UPDATE petrachenkovdb.usertime SET lastlogintime=?loginDate WHERE idusertime =?user;";
            string loginSql = $"SELECT idaccount FROM petrachenkovdb.account WHERE login=?user AND hashpassword=?password;";
            string GetUserDataSql = $"SELECT name,patronum,surname,description,dateofbirth,imageurl FROM petrachenkovdb.userdata WHERE iduserdata=?iduser";
            DateTime logintime = DateTime.Now;
            try
            {
                conn.Open();
                MySqlCommand logincommand = new MySqlCommand(loginSql, conn);
                logincommand.Parameters.Add("?user", MySqlDbType.VarChar).Value = $"{Login}";
                logincommand.Parameters.Add("?password", MySqlDbType.VarChar).Value = $"{GetCryptPassword(Login, UserPassword)}";
                object result = logincommand.ExecuteScalar();
                // Если результат null, значит пользователь не найден, или пароль неверный
                if (result == null)
                {
                    MessageBox.Show("Неверный логин или пароль!");
                    return false;
                }
                else
                {
                    UserData.UserId = (ulong)int.Parse(result.ToString());
                }
                try
                {
                    MySqlCommand updatetimesql = new MySqlCommand(updateTime, conn);
                    updatetimesql.Parameters.Add("?loginDate", MySqlDbType.DateTime).Value = DateTime.Now;
                    updatetimesql.Parameters.Add("?user", MySqlDbType.UInt64).Value = UserData.UserId;
                    updatetimesql.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Возникла ошибка при входе в аккаунт!\n{ex.Message}");
                }
                try
                {
                    MySqlCommand getUserDataCommand = new MySqlCommand(GetUserDataSql, conn);
                    getUserDataCommand.Parameters.Add("?iduser", MySqlDbType.UInt64).Value = UserData.UserId;
                    MySqlDataReader reader = getUserDataCommand.ExecuteReader();
                    try
                    {
                        reader.Read();
                        if (reader.IsDBNull(0))
                        {
                            UserData.UserName = string.Empty;
                        }
                        else
                        {
                            UserData.UserName = reader.GetString(0);
                        }
                        if (reader.IsDBNull(1))
                        {
                            UserData.UserPatronum = string.Empty;
                        }
                        else
                        {
                            UserData.UserPatronum = reader.GetString(1);
                        }
                        if (reader.IsDBNull(2))
                        {
                            UserData.UserSurname = string.Empty;
                        }
                        else
                        {
                            UserData.UserSurname = reader.GetString(2);
                        }
                        if (reader.IsDBNull(3))
                        {
                            UserData.UserDescription = string.Empty;
                        }
                        else
                        {
                            UserData.UserDescription = reader.GetString(3);
                        }
                        if (reader.IsDBNull(4))
                        {
                            UserData.DateOfBirth = DateTime.MinValue;
                        }
                        else
                        {
                            UserData.DateOfBirth = reader.GetDateTime(4);
                        }
                        UserData.ProfileImgPath = reader.GetString(5);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка при чтении данных\n{ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Возникла ошибка!\n{ex.Message}");
                }
                UserData.LastLoginTime = logintime;
                UserData.UserLogin = Login;
                MessageBox.Show("Успешный вход!");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Произошла ошибка!\n{e.Message}");
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region Смена фотографии

        public static bool ChangeUserPhoto(String ImgPath)
        {
            string updatePhotoSql = $"UPDATE petrachenkovdb.userdata SET imageurl=?newImgPath WHERE iduserdata=?iduser;";
            MySqlCommand updatePhotocommand = new MySqlCommand(updatePhotoSql, conn);
            try
            {
                conn.Open();
                updatePhotocommand.Parameters.Add("?iduser", MySqlDbType.UInt64).Value = UserData.UserId;
                updatePhotocommand.Parameters.Add("?newImgPath", MySqlDbType.VarChar).Value = $"{ImgPath}";
                updatePhotocommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Произошла ошибка!\n{e.Message}");
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region Удаление аккаунта
        public static bool DeleteAccountFunction()
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите удалить ваш аккаунт?", "Подтвердите удаление", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    if (DeleteFromDatabase())
                    {
                        Login login = new Login();
                        login.Show();
                        MessageBox.Show("Успешно удалён аккаунт!");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Операция удаления аккаунта завершена неудачно!");
                        return false;
                    }
                case MessageBoxResult.No:
                    return false;
                default:
                    return false;
            }
        }
        private static bool DeleteFromDatabase()
        {
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            string sql = $"DELETE FROM petrachenkovdb.account where idaccount=?userid limit 1";
            try
            {
                MySqlCommand command = new MySqlCommand(sql, conn);
                command.Parameters.Add("?userid", MySqlDbType.UInt64).Value = UserData.UserId;
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка\n" + ex.Message);
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region Изменение информации о пользователе
        internal void ChangeUserData(string Surname, string Name, string Patronum, string Description, DateTime? DateOfBirth)
        {
            string UpdateDatasql = $"UPDATE petrachenkovdb.userdata SET name=?name,surname=?surname,patronum=?patronum,description=?description,dateofbirth=?dateofbirth WHERE iduserdata=?iduser;";
            MySqlCommand command = new MySqlCommand(UpdateDatasql, conn);
            try
            {
                conn.Open();
                command.Parameters.Add("?iduser", MySqlDbType.UInt64).Value = UserData.UserId;
                command.Parameters.Add("?name", MySqlDbType.VarChar).Value = Name;
                command.Parameters.Add("?surname", MySqlDbType.VarChar).Value = Surname;
                command.Parameters.Add("?patronum", MySqlDbType.VarChar).Value = Patronum;
                command.Parameters.Add("?description", MySqlDbType.VarChar).Value = Description;
                command.Parameters.Add("?dateofbirth", MySqlDbType.DateTime).Value = DateOfBirth;
                command.ExecuteNonQuery();
                MessageBox.Show("Данные были успешно изменены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при попытке изменения пользователя!\n{ex.Message}");
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        #region Поиск информации о пользователях
        public static ObservableCollection<User> SearchUser(string searchString)
        {
            string sql = "SELECT idaccount FROM petrachenkovdb.account WHERE login LIKE CONCAT('%',@string,'%') OR idaccount LIKE CONCAT ('%',@string,'%') UNION DISTINCT SELECT iduserdata FROM petrachenkovdb.userdata WHERE name LIKE CONCAT('%',@string,'%') OR patronum LIKE CONCAT('%',@string,'%') OR surname LIKE CONCAT('%',@string,'%');";
            string GetDataSql = $"SELECT name,patronum,surname,description,dateofbirth,imageurl FROM petrachenkovdb.userdata WHERE iduserdata=?iduser";
            string GetLoginSql = $"SELECT login FROM petrachenkovdb.account WHERE idaccount=?idaccount";
            MySqlCommand command = new MySqlCommand(sql, conn);
            MySqlDataReader reader;
            MySqlDataReader readerData;
            command.Parameters.Add("@string", MySqlDbType.VarChar).Value = searchString;
            ObservableCollection<User> tempUsers = new ObservableCollection<User>();
            conn.Open();
            try
            {
                List<long> IDs = new List<long>();
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    long i = long.Parse(reader.GetString(0));
                    IDs.Add(i);
                }
                reader.Close();
                for (int i = 0; i < IDs.Count; i++)
                {
                    User user = new User();
                    MySqlCommand getDataCommand = new MySqlCommand(GetDataSql, conn);
                    MySqlCommand getLoginCommand = new MySqlCommand(GetLoginSql, conn);
                    getLoginCommand.Parameters.Add("?idaccount", MySqlDbType.UInt64).Value = IDs[i];
                    user.UserLogin = getLoginCommand.ExecuteScalar().ToString();
                    getDataCommand.Parameters.Add("?iduser", MySqlDbType.UInt64).Value = IDs[i];
                    readerData = getDataCommand.ExecuteReader();
                    try
                    {
                        readerData.Read();
                        if (readerData.IsDBNull(0))
                        {
                            user.UserName = string.Empty;
                        }
                        else
                        {
                            user.UserName = readerData.GetString(0);
                        }
                        if (readerData.IsDBNull(1))
                        {
                            user.UserPatronum = string.Empty;
                        }
                        else
                        {
                            user.UserPatronum = readerData.GetString(1);
                        }
                        if (readerData.IsDBNull(2))
                        {
                            user.UserSurname = string.Empty;
                        }
                        else
                        {
                            user.UserSurname = readerData.GetString(2);
                        }
                        if (readerData.IsDBNull(3))
                        {
                            user.UserDescription = string.Empty;
                        }
                        else
                        {
                            user.UserDescription = readerData.GetString(3);
                        }
                        if (readerData.IsDBNull(4))
                        {
                            user.DateOfBirth = DateTime.MinValue;
                        }
                        else
                        {
                            user.DateOfBirth = readerData.GetDateTime(4);
                        }
                        user.ProfileImgPath = readerData.GetString(5);
                        readerData.Close();
                        tempUsers.Add(user);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Произошла ошибка при чтении данных\n{ex.Message}");
                    }
                }
                if (tempUsers.Count == 0)
                {
                    MessageBox.Show("Пользователей не найдено");
                    return tempUsers;
                }
                else
                {
                    return tempUsers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка\n{ex.Message}");
                return tempUsers;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion
    }
}
