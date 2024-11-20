using System;

namespace socketchat
{
    public class Message
    {
        public DateTime DateOfMessage { get; set; }
        public string Content { get; set; }
        public string ImgProfilePath { get; set; }
        public string UserLogin { get; set; }
        public ulong UserId { get; set; }
        public Message(DateTime DateOfMessage, string Content, string ImgProfilePath, string UserLogin, ulong UserId)
        {
            this.DateOfMessage = DateOfMessage;
            this.Content = Content;
            this.ImgProfilePath = ImgProfilePath;
            this.UserLogin = UserLogin;
            this.UserId = UserId;
        }
    }
}