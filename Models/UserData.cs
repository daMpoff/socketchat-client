using System;

namespace socketchat
{
    public static class UserData
    {
        public static ulong UserId { get; set; }
        public static string UserLogin { get; set; }
        public static DateTime LastLoginTime { get; set; }
        public static DateTime RegisterTime { get; set; }
        public static DateTime DateOfBirth { get; set; }
        public static string ProfileImgPath { get; set; }
        public static string UserSurname { get; set; }
        public static string UserName { get; set; }
        public static string UserPatronum { get; set; }
        public static string UserDescription { get; set; }
    }
}