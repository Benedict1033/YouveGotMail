using Dapper;
using HelloAuth_Model.Data;
using HelloAuth_Utility;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Windows.Forms;
using static YouveGotMail.Controllers.EmailController;

namespace YouveGotMail.Controllers
{
    [RoutePrefix("api")]
    public class TeamsController : ApiController
    {

        //[HttpGet]
        //[Route("test")]
        //public async Task  test() {
        //    var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    string url = "https://prod-38.southeastasia.logic.azure.com:443/workflows/a04ca71d89b348119028b89b5c479cb4/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=-_3zLapiWYoqIUbP4cW9hyCN0d2fWFvIQMRycvCPoJI";
        //    //string url = "https://prod-38.southeastasia.logic.azure.com:443/workflows/df8cd341e52f48cbbff27f7eddac21b3/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=LBrdHFHcuCghPxJnc_fAjV60zirhBvs7IEf22qHENYc";

        //    var content = new StringContent("", Encoding.UTF8, "application/json");
        //    using (var response = await httpClient.PostAsync(url, content))
        //    {
        //        response.EnsureSuccessStatusCode();
        //    }

        //}


        ObjectCache cache = MemoryCache.Default;

        [HttpPost]
        [Route("approval")]
        public async Task<string> PostTeamsApproveAsync(TeamsApprove model)
        {
            var token = HttpContext.Current.Request.Headers["Authorization"];
            var secretKey = ConfigurationManager.AppSettings["TokenKey"];

            if (CheckAccessToken.CheckToken(token, secretKey) || model.system == "AdminTesting")
            {
                string[] to_Num = model.toEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None); //2種分隔符號

                for (int i = 0; i < to_Num.Length; i++)
                {
                    model.toEmail = to_Num[i].ToLower().Contains("@makalot.com.tw") ? to_Num[i].ToLower().Replace("@makalot.com.tw", "@Makalot.com") : to_Num[i];

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //string url = "https://prod-26.southeastasia.logic.azure.com:443/workflows/f6011522dfb84732bc7421a7c18a823f/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=UAfoYBN2ODAJBqqkiX7iYRSG9xVfEn0XEmQeLex5mZ8";
                    string url = "https://prod-46.southeastasia.logic.azure.com:443/workflows/b36dddaa9fc946a7b2a4b6165f5be612/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=wxfNgVtxIgnWRI8RB4pvqidCJQiHQZg781TU1NZRFsE";

                    string jsonData = JsonConvert.SerializeObject(model);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(url, content))
                    {
                        response.EnsureSuccessStatusCode();
                    }

                    await Task.Delay(1000);
                }
                return "ok";
            }
            else
                return "token fail";
        }



        //[HttpPost]
        //[Route("id")]
        //public string messageId(id id)
        //{
        //    return id.message;

        //}

        [HttpPost]
        [Route("teams")]
        public async Task<MessageClass> PostTeamsAsync(Teams model)
        {
            if (model.btnName != null && model.url != null)
            {
                for (int i = 0; i < model.btnName.Length; i++)
                {
                    model.btnName[i] = "{ \"type\": \"Action.OpenUrl\", \"title\": \"" + model.btnName[i] + "\", \"url\": \"" + model.url[i] + "\" },";
                    model.body = model.body + model.btnName[i];
                }
            }

            var token = HttpContext.Current.Request.Headers["Authorization"];
            var secretKey = ConfigurationManager.AppSettings["TokenKey"];
            string messageid = "";


            if (CheckAccessToken.CheckToken(token, secretKey) || model.system == "AdminTesting")
            {
                string[] to_Num = model.toEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None); //2種分隔符號

                for (int i = 0; i < to_Num.Length; i++)
                {
                    model.toEmail = to_Num[i].ToLower().Contains("@makalot.com.tw") ? to_Num[i].ToLower().Replace("@makalot.com.tw", "@Makalot.com") : to_Num[i];

                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = "";
                    if (model.messageid != "")
                    {
                        url = "https://prod-29.southeastasia.logic.azure.com:443/workflows/7e063fbb34104d0db4a535b46a9d7b03/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=wwGlmorP9WjsJyQ5VL90PUZziBpAQO28VKTtk14wLfE";
                    }
                    else
                    {

                        url = "https://prod-28.southeastasia.logic.azure.com:443/workflows/eb76f7623b174d9bb0c75cfb50afaf91/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=u_kvhEkUUUrIVEEmoBMxFhE3ZWfDsK7xqE2wmCih-Iw";
                        //string url = "https://prod-38.southeastasia.logic.azure.com:443/workflows/df8cd341e52f48cbbff27f7eddac21b3/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=LBrdHFHcuCghPxJnc_fAjV60zirhBvs7IEf22qHENYc";
                    }
                    string jsonData = JsonConvert.SerializeObject(model);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(url, content))
                    {

                        response.EnsureSuccessStatusCode();
                        if (response.Headers.TryGetValues("ID", out var headerValues))
                        {
                            messageid = headerValues.FirstOrDefault();
                            Console.WriteLine($"Header Value: {messageid}");
                        }

                    }

                    await Task.Delay(1000);
                }


                return new MessageClass
                {
                    result = "Success",
                    messageId= messageid,
                };
            }
            else
                return new MessageClass
                {
                    result = "Token fail",
                    messageId = messageid,
                };


        }

        [HttpPost]
        [Route("graphTeams")]
        public dataResults<bool> Post(TeamsMsg model)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var secretKey = ConfigurationManager.AppSettings["TokenKey"];
                //if (Token.CheckAccessToken(token))
                if (CheckAccessToken.CheckToken(token, secretKey) || model.system == "AdminTesting")
                {
                    string fromEmail = model.fromEmail.ToLower().Contains("@makalot.com.tw") ? model.fromEmail.ToLower().Replace("@makalot.com.tw", "@Makalot.com") : model.fromEmail;
                    var task = Task.Run(async () => await GetATokenForGraph(fromEmail)).Result;
                    string graphToken = task;
                    //string[] to_Num = model.toEmail.Split(',');
                    string[] to_Num = model.toEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None); //2種分隔符號
                    int successSend = 0;
                    for (int i = 0; i < to_Num.Length; i++) //若to超過1人，就會進for多人傳送
                    {
                        string toEmail = to_Num[i].ToLower().Contains("@makalot.com.tw") ? to_Num[i].ToLower().Replace("@makalot.com.tw", "@Makalot.com") : to_Num[i];
                        SeriLog.SaveSeriLog("Teams", "ToEmail - " + toEmail + ", TeamsMsg - " + model.teamsMsg);
                        //string final_Content = model.teamsMsg.Replace("'", "\\\'");
                        string Mail_content = "";

                        using (var client = new HttpClient())
                        {
                            // 發送Card只需把json body貼在teamsMsg,可參考範例三 --> https://learn.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-4-file-attachments&preserve-view=true
                            // 發送Attachment只需把json body貼在teamsMsg,可參考範例四 --> https://learn.microsoft.com/en-us/graph/api/chatmessage-post?view=graph-rest-beta&tabs=http#example-4-file-attachments&preserve-view=true

                            switch (model.type.ToLower())
                            {
                                case "html":
                                    Mail_content = "{\"body\":{\"contentType\":\"html\",\"content\":\"" + model.teamsMsg + "\"}}";
                                    break;

                                case "important":
                                    Mail_content = "{\"importance\": \"high\" ,\"body\":{\"contentType\":\"html\",\"content\":\"" + model.teamsMsg + "\"}}";
                                    break;

                                case "card":
                                    Mail_content = "{\"body\":{\"contentType\":\"html\",\"content\":\"<attachment id=\\\"A\\\"></attachment>\"}," +
                                        "\"attachments\":[" +
                                        "{\"id\":\"A\",\"contentType\":\"application/vnd.microsoft.card.thumbnail\"," +
                                        "\"content\":\"" + model.teamsMsg + "\"}]}";
                                    break;

                                case "card2":
                                    Mail_content = "{\"body\":{\"contentType\":\"html\",\"content\":\"<attachment id=\\\"A\\\"></attachment>\"}," +
                                        "\"attachments\":[" +
                                        "{\"id\":\"A\",\"contentType\":\"application/vnd.microsoft.card.adaptive\"," +
                                        "\"content\":\"" + model.teamsMsg + "\"}]}";
                                    break;

                                case "custom":
                                    Mail_content = model.teamsMsg;
                                    break;

                                default:
                                    Mail_content = "{\"body\":{\"contentType\":\"html\",\"content\":\"" + "<b>=" + model.system + "系統通知=</b><br/>" + "" + model.teamsMsg + "\"}}";
                                    break;
                            }

                            string chatId = Task.Run(async () => await get_Chat_Id(fromEmail, toEmail, graphToken)).Result;
                            if (!string.IsNullOrEmpty(chatId))
                            {
                                string endpoint = "https://graph.microsoft.com/v1.0/chats/" + chatId + "/messages"; // API endpoint 
                                var json = JObject.Parse(Mail_content);
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graphToken);
                                var response = client.PostAsJsonAsync(endpoint, json).Result.Content.ReadAsStringAsync();
                                successSend++;
                                SeriLog.SaveSeriLog("Teams", "Success: ToEmail - " + toEmail);
                            }
                            else
                            {
                                //SeriLog.SaveSeriLog("Teams", "Fail: get_chat_id - " + toEmail);
                                return new dataResults<bool>
                                {
                                    Message = "Get Chat Id Fail.",
                                    Code = "200",
                                    Data = false,
                                    Success = false
                                };
                            }
                        }
                    }

                    //Teams_Log(model, "成功", "");  //存成功的Log
                    //return Ok("Success");
                    if (successSend == to_Num.Length)
                        return new dataResults<bool>
                        {
                            Message = "ok",
                            Code = "200",
                            Data = true,
                            Success = true
                        };
                    else
                        return new dataResults<bool>
                        {
                            Message = "Teams Message Lost.",
                            Code = "200",
                            Data = true,
                            Success = false
                        };
                }
                else
                {
                    //Teams_Log(model, "失敗", "Access Token Fail"); //存失敗的Log + exception
                    //return BadRequest("Access Token Fail");
                    SeriLog.SaveSeriLog("Teams", "Fail: Invalid Token. " + model.toEmail);
                    return new dataResults<bool>
                    {
                        Message = "Invalid Token",
                        Code = "200",
                        Data = false,
                        Success = false
                    };
                }
            }
            catch (Exception ex)
            {
                //Teams_Log(model, "失敗", ex.ToString()); //存失敗的Log + exception
                //return BadRequest(ex.Message + ";" + ex.StackTrace);
                return new dataResults<bool>
                {
                    Message = ex.ToString(),
                    Code = "200",
                    Data = false,
                    Success = false
                };
            }
        }


        private async Task<string> GetATokenForGraph(string fromEmail)   // 取得 Acess Token
        {
            //string authority = "https://login.microsoftonline.com/dfb5e216-2b8a-4b32-b1cb-e786a1095218/"; //設定authority https://login.microsoftonline.com/{tenent}/
            string tenent = ConfigurationManager.AppSettings["GraphAPITenent"];
            //string authority = "https://login.microsoftonline.com/20499a37-7dae-4d58-acd3-7715bdd94be1/";
            string authority = "https://login.microsoftonline.com/" + tenent + "/";

            string[] scopes = new string[] { "openid" }; // 設定scopes

            IPublicClientApplication app;
            //app = PublicClientApplicationBuilder.Create("664c4b50-f060-4012-a792-4da48db6208d").WithAuthority(authority).Build(); //Create(client_Id)
            //app = PublicClientApplicationBuilder.Create("d12cf3d5-8a1c-41c3-8795-d70608f6b304").WithAuthority(authority).Build(); //Create(client_Id)
            string clineId = System.Configuration.ConfigurationManager.AppSettings["GraphAPIClientId"];
            app = PublicClientApplicationBuilder.Create(clineId).WithAuthority(authority).Build(); //Create(client_Id)
            var accounts = await app.GetAccountsAsync();
            AuthenticationResult result = null;
            //int loopCount = 0;

            if (accounts.Any())
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
            else
            {
                try
                {
                    if (cache["token"] == null)
                    {
                        result = null;
                        result = await app.AcquireTokenByUsernamePassword(scopes, fromEmail, "M@k@10t1477").ExecuteAsync();

                        var policy = new CacheItemPolicy();
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["CacheExpireSecond"]));//多久時間清除快取項目
                        cache.Set("token", result.AccessToken, policy);
                        Thread.Sleep(1000);
                        return cache["token"].ToString();
                    }
                    else
                        return cache["token"].ToString();
                }
                catch (Exception ex)
                {
                    SeriLog.SaveSeriLog("GetATokenForGraph", "Exception: " + ex.Message);
                    Console.WriteLine(ex);
                }
            }

            if (result != null)
            {
                SeriLog.SaveSeriLog("GetATokenForGraph", "Success: " + result.AccessToken);
                return result.AccessToken;
            }
            else
            {
                SeriLog.SaveSeriLog("GetATokenForGraph", "Get AccessToken Fail");
                return "";
            }
        }

        private async Task<string> get_Chat_Id(string fromEmail, string toEmail, string graphToken) // 取得chat_Id
        {
            try
            {
                var endpoint = "";
                string bd = "";
                HttpResponseHeaders header = null;
                string chatId = "";
                int loopCount = 0;
                string result = "";
                JObject res = null;
                using (var client = new HttpClient())
                {
                    SeriLog.SaveSeriLog("get_Chat_Id", "Get Chat Id Start.");
                    do
                    {
                        endpoint = "https://graph.microsoft.com/v1.0/chats";
                        bd = "{\"chatType\":\"oneOnOne\",\"members\":[{\"@odata.type\":\"#microsoft.graph.aadUserConversationMember\",\"roles\":[\"owner\"],\"user@odata.bind\":" +
                            "\"https://graph.microsoft.com/v1.0/users('" + fromEmail + "')\"}," +  // 自己的帳號
                            "{\"@odata.type\":\"#microsoft.graph.aadUserConversationMember\",\"roles\":[\"owner\"],\"user@odata.bind\":" +
                            "\"https://graph.microsoft.com/v1.0/users('" + toEmail + "')\"}]}"; // 收件者的帳號

                        JObject json = JObject.Parse(bd);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", graphToken);
                        HttpResponseMessage responseMessage = client.PostAsJsonAsync(endpoint, json).Result;
                        result = responseMessage.Content.ReadAsStringAsync().Result;
                        header = responseMessage.Headers;
                        //string result = client.PostAsJsonAsync(endpoint, json).Result.Content.ReadAsStringAsync().Result;
                        //string result = client.PostAsJsonAsync(endpoint, json).Result.Headers;                        
                        res = JObject.Parse(result);

                        //if (string.IsNullOrEmpty(chatId.Trim()) && loopCount < 5)
                        if (res.ContainsKey("id") == false && loopCount < 5)
                            //Thread.Sleep(3000);
                            await Delay(10000);
                        else
                            chatId = (string)res["id"];
                        loopCount++;
                        SeriLog.SaveSeriLog("get_Chat_Id", "\nResponseHeader: " + header + "loopCount: " + loopCount);
                    } while (string.IsNullOrEmpty(chatId) && loopCount < 5);

                    if (res.ContainsKey("id") == false)
                    {
                        SeriLog.SaveSeriLog("get_Chat_Id", "Get Chat Id Fail Email: " + result);
                        MailRequest mail = new MailRequest
                        {
                            ToEmail = "benedicttiong@makalot.com.tw",
                            Subject = "[Postino] Get Chat Id Fail. " + DateTime.Now.ToString(),
                            Body = "After " + loopCount + " tries still can' get chat id. \nTo Email: " + toEmail + "\nGraph API Token: " + graphToken + "\nResponseHeader: " + header +
                                   "\nEndpoint: " + endpoint + "\nJson: " + bd
                        };
                        EmailController emailController = new EmailController();
                        emailController.SendEmail(mail);
                    }
                    else
                        SeriLog.SaveSeriLog("get_Chat_Id", "Get Chat Id Success: '" + chatId + "', IsNullOrEmpty: " + string.IsNullOrEmpty(chatId) + ",loopCount: " + loopCount);
                    return chatId; // return chat_Id
                }
            }
            catch (Exception ex)
            {
                SeriLog.SaveSeriLog("get_Chat_Id", "Exception: " + ex.Message);
                throw ex;
            }
        }

        static async System.Threading.Tasks.Task Delay(int iSecond)
        {
            SeriLog.SaveSeriLog("Delay", "\nmilliseconds: " + iSecond);
            await System.Threading.Tasks.Task.Delay(iSecond);
        }

        void Teams_Log(TeamsMsg model, string status, string ex)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); ;
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string[] to_Num = model.toEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None);
            string final_Mail = "";

            for (int i = 0; i < to_Num.Length; i++)
            {
                string temp = to_Num[i].ToString();
                final_Mail = temp + "\r\n" + final_Mail; //用.csv打開若多個mail_To會換行顯示
            }
            try
            {
                var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
                con.Open();
                var query = "INSERT INTO YouveGotMail.dbo.teams(from_System,Mail_To,Mail_Content,IP_Address,Access_DateTime,Access_Status,Exception) " +
                            "VALUES(@from_System,@Mail_To,@Mail_Content,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";
                var dp = new DynamicParameters();

                dp.Add("@from_System", model.system);
                dp.Add("@Mail_To", final_Mail);
                dp.Add("@Mail_Content", model.teamsMsg);
                dp.Add("@IP_Address", ip);
                dp.Add("@Access_DateTime", time);
                dp.Add("@Access_Status", status);
                dp.Add("@Exception", ex);
                con.Execute(query, dp);
            }
            catch (Exception ex2)
            {
                throw ex2;
            }
        }
    }
    public class TeamsMsg
    {
        public string system { get; set; }
        public string toEmail { get; set; }
        public string fromEmail { get; set; }
        public string teamsMsg { get; set; }
        public string type { get; set; }
    }

    public class Teams
    {
        public string system { get; set; }
        public string toEmail { get; set; }
        public string teamsMsg { get; set; }
        public string type { get; set; }
        public string title { get; set; } = "";
        public string body { get; set; } = "";
        public string[] btnName { get; set; } = new string[0];
        public string[] url { get; set; } = new string[0];
        public string messageid { get; set; } = "";
        public string cardStatus { get; set; } = "";
    }

    public class TeamsApprove
    {
        public string system { get; set; }
        public string toEmail { get; set; }
        public string teamsMsg { get; set; }
        public string approveUrl { get; set; }
        public string rejectUrl { get; set; }
    }

    public class MessageClass
    {
        public string result { get; set; }
        public string messageId { get; set; }
    }
}