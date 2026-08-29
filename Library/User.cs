using System;

namespace VirtualEMS.Library
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        //public string PasswordSalt { get; set; }
    }
}
