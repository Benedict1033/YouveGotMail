using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using YouveGotMail.Controllers;

namespace YouveGotMail
{
    public class Token
    {
        private static string key = ConfigurationManager.AppSettings["TokenKey"];
        public static dataJWTTOKEN GetAccessToken(tokenClass model)
        {
            //Byte[] bytesEncode = System.Text.Encoding.UTF8.GetBytes("SmartBPM20221107"); //取得 UTF8 2進位 Byte
            //string resultEncode = Convert.ToBase64String(bytesEncode); // 轉換 Base64 索引表
            try
            {
                var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
                con.Open();
                var query = "Select System,Password FROM  YouveGotMail.dbo.System WHERE System ='" + model.system + "'";
                SqlCommand command = new SqlCommand(query, con);
                SqlDataReader reader = command.ExecuteReader();
                string password = "";

                while (reader.Read())
                {
                   password = reader["Password"].ToString();
                }


                string encode = Encoding.UTF8.GetString(Convert.FromBase64String(model.passWord));
                string pass = Encoding.UTF8.GetString(Convert.FromBase64String(password));
               

                if (encode != pass)
                    return new dataJWTTOKEN
                    {
                        accessToken = ""
                    };

                var exp = 10 * 60;   //過期時間(秒)

                //稍微修改 Payload 將使用者資訊和過期時間分開
                var payload = new dataPayload
                {
                    system = model.system,
                    //Unix 時間戳
                    exp = Convert.ToInt32(
                        (DateTime.Now.AddSeconds(exp) -
                         new DateTime(1970, 1, 1)).TotalSeconds)
                };

                var json = JsonConvert.SerializeObject(payload);
                var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                var iv = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

                //使用 AES 加密 Payload
                var encrypt = Cryptography.AesEncrypt(base64Payload, key.Substring(0, 32), iv);
                //var encrypt = Cryptography.AesEncrypt(encode.Substring(0, 16), iv, base64Payload);
                

                //取得簽章
                var signature = Cryptography.ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0, 64));

                return new dataJWTTOKEN
                {
                    //Token 為 iv + encrypt + signature，並用 . 串聯
                    accessToken = iv + "." + encrypt + "." + signature,
                    //Refresh Token 使用 Guid 產生
                    refreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                    expiresIn = exp,
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }
        public static bool CheckAccessToken(string token)
        {
            try
            {
                //FormsAuthenticationTicket encryptedTicket1 = FormsAuthentication.Decrypt(token);
                //userName = encryptedTicket1.UserData;

                //if (encryptedTicket1.Expired)
                //    return "@error@";
                //else
                //    return encryptedTicket1.Name;
                //token = System.Web.HttpUtility.UrlDecode(token);
                //string encode = Encoding.UTF8.GetString(Convert.FromBase64String(passWord));
                var split = token.Split('.');
                var iv = split[0];
                var encrypt = split[1];
                var signature = split[2];

                //檢查簽章是否正確
                if (signature != Cryptography.ComputeHMACSHA256(iv + "." + encrypt, key.Substring(0,64)))
                {
                    return false;
                }

                //使用 AES 解密 Payload
                var base64 = Cryptography.AesDecrypt(encrypt, key.Substring(0, 32), iv);
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                var payload = JsonConvert.DeserializeObject<dataPayload>(json);

                //檢查是否過期
                if (payload.exp < Convert.ToInt64(
                    (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
                //Response.Write("[CheckToken]error<br>token=" + token+"<br>ex:" + ex.Message.ToString());//kisatest
                //return "@error@";
            }
        }
    }
    public class dataJWTTOKEN
    {
        //Token
        public string accessToken { get; set; }
        //Refresh Token
        public string refreshToken { get; set; }
        //幾秒過期
        public int expiresIn { get; set; }
    }
    public class dataPayload
    {
        //使用者資訊
        //public string DISPLAY_NAME { get; set; }
        public string system { get; set; }
        //過期時間
        public int exp { get; set; }
    }
}