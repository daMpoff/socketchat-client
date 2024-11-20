using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace socketchat.Models
{
    public static class WorkWithImageApi
    {
        static string apiurl = "https://api.imgur.com/3/image";//адрес api сервиса Imgur

        #region Смена аватарки пользователя
        public static async Task ChangeProfileImageAsync(ImageBrush ProfileImage)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Выберите новое изображения профиля";
                openFileDialog.Filter = "Все типы изображений |*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "PNG (*.png)|*.png";

                if (openFileDialog.ShowDialog() == true)
                {
                    if (await UploadProfileAvatar(openFileDialog.FileName))
                    {
                        BitmapImage image = new BitmapImage();
                        image.BeginInit();
                        image.UriSource = new Uri(openFileDialog.FileName);
                        image.EndInit();
                        ProfileImage.ImageSource = image;
                        MessageBox.Show("Успешная загрузка!");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Возникла ошибка при загрузке изображения!\n{e.Message}");
            }
        }
        #endregion

        #region Получение ссылки на изображение,загруженное на хостинг
        public static async Task<string> GetUrlImageAsync(string imgpath)
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiurl);
                request.Headers.Add("Authorization", "Client-ID d56c97396bcab7d");
                MultipartFormDataContent content = new MultipartFormDataContent();
                string base64 = Convert.ToBase64String(File.ReadAllBytes(imgpath));
                content.Add(new StringContent(base64), "image");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
                return (string)result["data"]["link"]; //парсим строку из json ответа,чтобы вытащить новый url
            }
            catch
            {
                MessageBox.Show("Возникла ошибка при загрузке данных на хостинг изображений!");
                return null;
            }
            finally
            {
                client.Dispose();
            }
        }
        #endregion

        #region Загрузки аватарки пользователя 
        private static async Task<bool> UploadProfileAvatar(string imgpath)
        {
            string ImgUrl = await GetUrlImageAsync(imgpath);
            try
            {
                if ((ImgUrl != null) && DBCRUD.ChangeUserPhoto(ImgUrl))
                {
                    UserData.ProfileImgPath = ImgUrl;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Возникла ошибка при загрузке изображения в базу данных!\n{e.Message}");
                return false;
            }
        }
        #endregion
    }
}