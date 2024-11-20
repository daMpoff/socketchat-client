using System;

namespace socketchat
{
    public class ImageMessage
    {
        public DateTime DateOfMessage { get; set; }
        public string ImgContentPath { get; set; }
        public string ImgProfilePath { get; set; }
        public string UserLogin { get; set; }
        public ulong UserId { get; set; }
        public ImageMessage(DateTime DateOfMessage, string ImgContent, string ImgProfilePath, string UserLogin, ulong UserId)
        {
            this.DateOfMessage = DateOfMessage;
            this.ImgContentPath = ImgContent;
            this.ImgProfilePath = ImgProfilePath;
            this.UserLogin = UserLogin;
            this.UserId = UserId;
        }
    }
}
