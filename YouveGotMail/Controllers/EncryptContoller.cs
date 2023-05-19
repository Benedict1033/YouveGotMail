using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Specialized;
using System.Net.Http;
using Serilog.Core;
using System.Reflection;

namespace YouveGotMail.Controllers
{
    [EnableCors("*", "*", "*")]

    [RoutePrefix("api")]
    public class EncryptController : ApiController
    {
        private static string key = ConfigurationManager.AppSettings["EncryptKey"];
        [HttpPost]
        [Route("encrypt")]
        public EncryptClass Encrypt(EncryptClass model)
        {
            try
            {
                string encryptString = "";
                //var iv = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                byte[] bytesDate = System.Text.Encoding.GetEncoding("utf-8").GetBytes(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
                var iv = Convert.ToBase64String(bytesDate);
                //使用 AES 加密 string
                encryptString = Cryptography.AesEncrypt(model.originString, key.Substring(0, 32), iv.Substring(0, 16));

                var encryptInfo = new EncryptClass
                {
                    originString = model.originString,
                    //Unix 時間戳
                    encryptString = encryptString,
                    iv = iv
                };
                return encryptInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("decrypt")]
        public HttpResponseMessage Decrypt(EncryptClass model)
        {
            try
            {
                string decryptString = "";

                string date = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(model.iv));
                DateTime expire = Convert.ToDateTime(date).AddDays(14);

                if (expire > DateTime.Now)
                {
                    //使用 AES 解密 string
                    decryptString = Cryptography.AesDecrypt(model.encryptString, key.Substring(0, 32), model.iv.Substring(0, 16));
                }
                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                    decryptString,
                    Encoding.UTF8,
                    "text/html"
                )
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public string Decrypt(EncryptClass model)
        //{
        //    try
        //    {
        //        string decryptString = "";

        //        string date = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(model.iv));
        //        DateTime expire = Convert.ToDateTime(date).AddDays(14);

        //        if(expire > DateTime.Now)
        //        {
        //            //使用 AES 解密 string
        //            decryptString = Cryptography.AesDecrypt(model.encryptString, key.Substring(0, 32), model.iv.Substring(0,16));
        //        }                
        //        return decryptString;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        [HttpPost]
        [Route("EncryptTest")]
        public string EncryptTest(EncryptClass model)
        {

            string errno = "00";
            try
            {
                //--------------------------------------------------------------------
                string BPM_Password = "M@kalotBPM";
                string timePW = BPM_Password + "_" + DateTime.UtcNow.Ticks;

                //編成 Base64 字串
                byte[] bytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(timePW);
                string BPM_64PW = Convert.ToBase64String(bytes); //這串即為需傳送的PASSWORD
                //--------------------------------------------------------------------
                string jsonToken = "";
                using (var client = new HttpClient())
                {
                    errno = "01.01";
                    //1.https://portal.makalot.com.tw/youvegotmail/api/accessToken
                    //設定基底URI
                    client.BaseAddress = new Uri("https://portal.makalot.com.tw/youvegotmail/api/accessToken");
                    errno = "01.02";
                    client.DefaultRequestHeaders.Accept.Clear();//一定要清空-否則會出現500(Internal Server Error)!!!
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    errno = "01.04";
                    //或採用如下
                    HttpRequestMessage request = new HttpRequestMessage();
                    request.Method = HttpMethod.Post;
                    errno = "01.03";
                    //request.RequestUri = new Uri("accessToken");
                    errno = "02";
                    //設定Header
                    //request.Headers.Add("Authorization", jsonToken);
                    //-------------------------------------------------------------
                    string[] scope = { "Postino" };

                    JSON_TOKEN_PARM json_teams = new JSON_TOKEN_PARM()
                    {
                        SYSTEM = "BPM",
                        PASSWORD = BPM_64PW,
                        SCOPE = new List<string>(scope)
                        //SCOPE =new string[] { "Postino" }
                    };
                    errno = "02.02";
                    string json = JsonConvert.SerializeObject(json_teams);
                    errno = "02.03";
                    //string json = @"{""SYSTEM"": ""ERP"", ""PASSWORD"": ""RW50ZXJwcmlzZTIwMjIwOTE5""}";   
                    errno = "03";
                    //設定body內容和格式
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    errno = "04";
                    var response = client.SendAsync(request).Result;
                    errno = "05";
                    //判斷是否連線成功
                    if (response.IsSuccessStatusCode)
                    {
                        errno = "06";
                        //取回傳值
                        //var APIResult = response.Content.ReadAsAsync<string>().Result;
                        //jsonToken =  response.Content.ReadAsStringAsync();
                        //JObject obj = (JObject)JsonConvert.DeserializeObject(response);

                        errno = "07";
                        return jsonToken;
                    }
                    else
                    {
                        errno = "08";
                        var responsecode = (int)response.StatusCode;
                        errno = "09";
                        return "Err:(GetToken)" + responsecode + " " + response.ReasonPhrase;
                        //return "response.IsSuccessStatusCode=false";
                    }



                };
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        [HttpPost]
        [Route("DecryptTest")]
        public string DecryptTest(EncryptClass model)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(@"http://192.168.4.65:8081/api/decrypt");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"encryptString\":\"" + model.encryptString + "\"," +
                              "\"iv\":\"" + model.iv + "\"}";
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string temp;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                temp = streamReader.ReadToEnd();
                //if (!string.IsNullOrEmpty(DecryptData))
                //{
                //    EncryptClass obj = JsonConvert.DeserializeObject<EncryptClass>(DecryptData);
                //    //JObject obj = (JObject)JsonConvert.DeserializeObject(result);
                //    //    return obj["originString"].ToString();
                //    return obj.originString;
                //}

            }
            return temp;



        }
    }

    public class EncryptClass
    {
        public string originString { get; set; }
        public string encryptString { get; set; }
        public string iv { get; set; }
    }
    public class JSON_TOKEN_PARM
    {
        public string SYSTEM { get; set; }
        public string PASSWORD { get; set; }
        public List<string> SCOPE { get; set; }
        //public string[] SCOPE { get; set; }
    }
}