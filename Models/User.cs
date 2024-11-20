using System;

namespace socketchat
{
    public class User
    {
        public ulong UserId { get; set; }
        public string UserLogin { get; set; }
        public DateTime LastLoginTime { get; set; }
        public DateTime RegisterTime { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ProfileImgPath { get; set; }
        public string UserSurname { get; set; }
        public string UserName { get; set; }
        public string UserPatronum { get; set; }
        public string UserDescription { get; set; }
    }
}
