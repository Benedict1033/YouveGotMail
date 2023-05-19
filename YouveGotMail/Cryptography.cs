using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace YouveGotMail
{
    public class Cryptography
    {
        /// <summary>
        /// 驗證key和iv的長度(AES只有三種長度適用)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        private static void Validate_KeyIV_Length(string key, string iv)
        {
            //驗證key和iv都必須為128bits或192bits或256bits
            List<int> LegalSizes = new List<int>() { 128, 192, 256 };
            int keyBitSize = Encoding.UTF8.GetBytes(key).Length * 8;
            int ivBitSize = Encoding.UTF8.GetBytes(iv).Length * 8;
            if (!LegalSizes.Contains(keyBitSize) || !LegalSizes.Contains(ivBitSize))
            {
                throw new Exception($@"key或iv的長度不在128bits、192bits、256bits其中一個，輸入的key bits:{keyBitSize},iv bits:{ivBitSize}");
            }
        }
        /// <summary>
        /// 加密後回傳base64String，相同明碼文字編碼後的base64String結果會相同(類似雜湊)，除非變更key或iv
        /// 如果key和iv忘記遺失的話，資料就解密不回來
        /// base64String若使用在Url的話，Web端記得做UrlEncode
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="plain_text"></param>
        /// <returns></returns>
        public static string AesEncrypt(string plain_text, string key, string iv)
        {
            Validate_KeyIV_Length(key, iv);
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            ICryptoTransform transform = aes.CreateEncryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));

            byte[] bPlainText = Encoding.UTF8.GetBytes(plain_text);//明碼文字轉byte[]
            byte[] outputData = transform.TransformFinalBlock(bPlainText, 0, bPlainText.Length);//加密
            return Convert.ToBase64String(outputData);
        }
        /// <summary>
        /// 解密後，回傳明碼文字
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static string AesDecrypt(string base64String,string key, string iv )
        {
            Validate_KeyIV_Length(key, iv);
            Aes aes = Aes.Create();
            //aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            ICryptoTransform transform = aes.CreateDecryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));
            byte[] bEnBase64String = null;
            byte[] outputData = null;
            try
            {
                bEnBase64String = Convert.FromBase64String(base64String);//有可能base64String格式錯誤
                outputData = transform.TransformFinalBlock(bEnBase64String, 0, bEnBase64String.Length);//有可能解密出錯
            }
            catch (Exception ex)
            {
                //todo 寫Log
                throw new Exception($@"解密出錯:{ex.Message}");
            }

            //解密成功
            return Encoding.UTF8.GetString(outputData);

        }

        //產生 HMACSHA256 雜湊
        public static string ComputeHMACSHA256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmacSHA = new HMACSHA256(keyBytes))
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var hash = hmacSHA.ComputeHash(dataBytes, 0, dataBytes.Length);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        public static string ToMD5(string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);

                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);

                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                    .Replace("-", String.Empty)
                    .ToUpper();

                return md5;
            }
        }

    }
}