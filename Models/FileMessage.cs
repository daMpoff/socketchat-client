using System;
using System.Security.Cryptography;
using System.Text;

namespace socketchat
{
    public class FileMessage
    {
        public DateTime DateOfMessage { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeDisplay { get; set; }
        public string FileContent { get; set; }
        public string Sha256 { get; set; }
        public string ImgProfilePath { get; set; }
        public string UserLogin { get; set; }
        public ulong UserId { get; set; }
        public static string GetSha256File(string filename, string filedata)
        {
            string salt = filename + filedata;
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
        public FileMessage(DateTime DateOfMessage, string fileName, long fileSize, string fileContent, string ImgProfilePath, string UserLogin, ulong UserId)
        {
            this.DateOfMessage = DateOfMessage;
            this.FileName = fileName;
            this.FileSize = fileSize;
            this.FileContent = fileContent;
            this.ImgProfilePath = ImgProfilePath;
            this.UserLogin = UserLogin;
            this.UserId = UserId;
            this.Sha256 = GetSha256File(fileName, fileContent);
            this.FileSizeDisplay = FileGetSize(fileSize);
        }
        private string FileGetSize(long fileSize)
        {
            if (fileSize < 1024)
            {
                return $"Размер файла: {fileSize} Б";
            }
            else if (fileSize < 1048576)
            {
                return $"Размер файла: {fileSize / 1024} Кб";
            }
            else return $"Размер файла: {fileSize / 1048576} Мб";
        }
    }
}
