using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace COMMON.Security
{
    public static class AES_Service
    {
        /// <summary>
        /// Mã hóa mảng byte dữ liệu bằng thuật toán AES.
        /// </summary>
        /// <param name="data">Dữ liệu thô cần mã hóa</param>
        /// <param name="key">Khóa AES (Shared Secret từ DH)</param>
        /// <returns>Mảng byte bao gồm IV + Dữ liệu đã mã hóa</returns>
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV(); // Tạo vector khởi tạo ngẫu nhiên cho mỗi lần mã hóa
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

                    // Kết hợp IV (16 bytes) vào đầu mảng kết quả để phía nhận có thể dùng giải mã
                    byte[] combinedData = new byte[aes.IV.Length + encryptedData.Length];
                    Buffer.BlockCopy(aes.IV, 0, combinedData, 0, aes.IV.Length);
                    Buffer.BlockCopy(encryptedData, 0, combinedData, aes.IV.Length, encryptedData.Length);

                    return combinedData;
                }
            }
        }

        /// <summary>
        /// Giải mã mảng byte dữ liệu (IV + EncryptedData).
        /// </summary>
        /// <param name="combinedData">Mảng byte nhận được từ sender</param>
        /// <param name="key">Khóa AES dùng chung</param>
        public static byte[] Decrypt(byte[] combinedData, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Tách IV từ 16 byte đầu tiên
                byte[] iv = new byte[16];
                byte[] encryptedData = new byte[combinedData.Length - 16];
                Buffer.BlockCopy(combinedData, 0, iv, 0, 16);
                Buffer.BlockCopy(combinedData, 16, encryptedData, 0, encryptedData.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                {
                    return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }
            }
        }

        // Helper: Mã hóa chuỗi văn bản
        public static byte[] EncryptString(string text, byte[] key)
        {
            return Encrypt(Encoding.UTF8.GetBytes(text), key);
        }

        // Helper: Giải mã về chuỗi văn bản
        public static string DecryptString(byte[] combinedData, byte[] key)
        {
            byte[] decryptedBytes = Decrypt(combinedData, key);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}