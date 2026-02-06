using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace COMMON.Security
{
    public static class CreateKey
    {
        public static byte[] GenerateRandomKey()
        {
            // 32 byte = 256 bit, phù hợp cho mã hóa AES-256
            byte[] key = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                // Điền các byte ngẫu nhiên bảo mật vào mảng
                rng.GetBytes(key);
            }
            return key;
        }
    }
}
