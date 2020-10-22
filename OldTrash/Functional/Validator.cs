using System;
using System.IO;
using System.Text;
using Common.Logging;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;

namespace Common.Functional.Pass
{
    public static class Validator
    {
		private const int MIN_LENGTH = 6;
        private const int MAX_LENGTH = 20;
        private static  EmailAddressAttribute foo = new EmailAddressAttribute();
		public static Random random = new Random();
        private static string Alphavite = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static string sum_names = "abc123";

        public static bool ValidateEmail(ref string email)
        {
            bool bar = foo.IsValid(email);
            Logger.WriteLog("Validating email=" + email + " success=" + bar, LogLevel.Usual);
            return bar;
        } 
		public static bool ValidatePassword(ref string password, ref string answer) 
		{
			if (string.IsNullOrEmpty(password)) 
            { 
                return false; 
            }
			bool meetsLengthRequirements = password.Length >= MIN_LENGTH && password.Length <= MAX_LENGTH;
			//bool hasUpperCaseLetter = false;
			bool hasLowerCaseLetter = false;
			//bool hasDecimalDigit = false;
			if (meetsLengthRequirements) 
			{
				foreach (char c in password)
				{
					if (char.IsUpper(c)) hasLowerCaseLetter = true;
					//else if (char.IsLower(c))  = true;
					//else if (char.IsDigit(c)) hasDecimalDigit = true;
				}
			}
            //if (hasUpperCaseLetter == false)
            {
            //    answer = "Current password does not has upper case letter.";
            }
            if (hasLowerCaseLetter == false)
            {
                answer = "Current password does not has lower case letter.";
            }
            //if (hasDecimalDigit == false)
            {
            //    answer = "Current password does not has decimal digit.";
            }
            bool isValid = meetsLengthRequirements  && hasLowerCaseLetter; // hasUpperCaseLetter && hasDecimalDigit

            Logger.WriteLog("Validate password success=" + isValid + ".", LogLevel.Usual);
			return isValid;         
        }
		public static bool EqualsPasswords(ref string password,ref string confirmpassword) 
		{
            bool answer = password.Equals(confirmpassword);
			Logger.WriteLog("Validating confirm password=" + answer + ".", LogLevel.Usual);
			return answer;
		}
        public static string GenerateHash(int length_hash)
        {
            string hash = "";
            for (int i = 0; i < length_hash; i++)
            {
                hash += Alphavite[random.Next(Alphavite.Length)];
            }
            return hash;
        }
        public static string HashPassword(ref string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                Logger.WriteLog("Input value is null, function HashPassword()", LogLevel.Error);
                return "";
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        public static bool VerifyHashedPassword(ref string hashedPassword,ref string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                return false;
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(ref buffer3,ref buffer4);
        }
        private static bool ByteArraysEqual(ref byte[] b1,ref byte[] b2)
        {
            if (b1 == b2)
            {
                return true;
            }
            if (b1 == null || b2 == null)
            { 
                return false; 
            }
            if (b1.Length != b2.Length)
            {
                return false;
            }
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
        public static string Encrypt(ref string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(sum_names,  new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(ref string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(sum_names, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
