﻿using System.ComponentModel.DataAnnotations;
using MiniMessanger.Models;

namespace mini_message.Dtos
{
    public struct UserDto
    {
        public int user_id;
        [Required]
        public string user_email;
        [Required]
        public string user_login;
        [Required]
        public string user_password;
        public string user_hash;
        public short activate;
        public int created_at;
        public string user_token;
        public int last_login_at;
        public int recovery_code;
        public string recovery_token;
        public ProfileData profile;
        public string user_public_token;
    }
}
