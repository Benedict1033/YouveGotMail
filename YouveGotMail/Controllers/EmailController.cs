using Dapper;
using MimeKit;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.Http;
using HttpContext = System.Web.HttpContext;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace YouveGotMail.Controllers
{
    [RoutePrefix("api")]
    public class EmailController : ApiController
    {
        [HttpPost]
        [Route("mail")]
        public void SendEmail(MailRequest model)
        {
            try
            {
                //附件處理
                Stream stream = null;

                if (model.FilePath == null || model.FilePath == "")
                    Console.WriteLine("no attachment");
                else
                {
                    FileStream fileStream = new FileStream(model.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, bytes.Length);
                    fileStream.Close();
                    stream = new MemoryStream(bytes);
                }

                //發送信件
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("系統通知", "notification@makalot.com.tw"));

                if (model.ToEmail != null)  //To
                {
                    string[] to_Num = model.ToEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None);
                    for (int i = 0; i < to_Num.Length; i++)
                        message.To.Add(new MailboxAddress("", to_Num[i]));
                }
                if (model.CcEmail != null) //Cc
                {
                    string[] cc_Num = model.CcEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None);
                    for (int i = 0; i < cc_Num.Length; i++)
                        message.Cc.Add(new MailboxAddress("", cc_Num[i]));
                }
                if (model.BccEmail != null) //Bcc
                {
                    string[] bcc_Num = model.BccEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None);
                    for (int i = 0; i < bcc_Num.Length; i++)
                        message.Bcc.Add(new MailboxAddress("", bcc_Num[i]));
                }

                var builder = new BodyBuilder();
                builder.TextBody = model.Body;

                if (model.FilePath == "" || model.FilePath == null)
                    Console.WriteLine("no attachment");
                else
                    builder.Attachments.Add("10.07進度報告.pptx", stream);

                //if (model.Attachments == null)
                //    Console.WriteLine("no attachment");
                //else
                //    builder.Attachments.Add("10.07進度報告.pptx", model.Attachments);

                message.Subject = model.Subject;
                message.Body = builder.ToMessageBody();

                var client = new SmtpClient();
                client.Connect("CAS1.MAKALOT.COM", 25);
                client.Send(message);
                client.Disconnect(true);

                //Mail_Log(model, "成功", ""); //Successful Mail Log
            }
            catch (Exception ex)
            {
                //Mail_Log(model, "失敗", ex.ToString()); //Unsuccessful Mail Log
            }
        }

        void Mail_Log(MailRequest model, string status, string ex)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); ;
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string final_Mail_To = "";
            string final_Mail_Cc = "";
            string final_Mail_Bcc = "";

            if (model.ToEmail != null) //To
            {
                string[] to_Num = model.ToEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None);
                for (int i = 0; i < to_Num.Length; i++)
                {
                    string temp = to_Num[i].ToString();
                    final_Mail_To = temp + "\r\n" + final_Mail_To; //用.csv打開若多個mail_To會換行顯示
                }
            }
            if (model.CcEmail != null) //Cc
            {
                string[] cc_Num = model.CcEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None);
                for (int i = 0; i < cc_Num.Length; i++)
                {
                    string temp = cc_Num[i].ToString();
                    final_Mail_Cc = temp + "\r\n" + final_Mail_Cc; //用.csv打開若多個mail_Cc會換行顯示
                }
            }
            if (model.BccEmail != null) //Bcc
            {
                string[] bcc_Num = model.BccEmail.Split(new string[] { ",", ";" }, StringSplitOptions.None);
                for (int i = 0; i < bcc_Num.Length; i++)
                {
                    string temp = bcc_Num[i].ToString();
                    final_Mail_Bcc = temp + "\r\n" + final_Mail_Bcc; //用.csv打開若多個mail_Bcc會換行顯示
                }
            }

            try
            {
                //daper的寫法
                var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
                con.Open();
                var query = "INSERT INTO YouveGotMail.dbo.mail(from_System,Mail_To,Mail_Cc,Mail_Bcc,Mail_Subject,Mail_Content,Mail_Attachment,IP_Address,Access_DateTime,Access_Status,Exception) " +
                            "VALUES(@from_System,@Mail_To,@Mail_Cc,@Mail_Bcc,@Mail_Subject,@Mail_Content,@Mail_Attachment,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";
                var dp = new DynamicParameters();

                dp.Add("@from_System", "ERP");
                dp.Add("@Mail_To", final_Mail_To);
                dp.Add("@Mail_Cc", final_Mail_Cc);
                dp.Add("@Mail_Bcc", final_Mail_Bcc);
                dp.Add("@Mail_Subject", model.Subject);
                dp.Add("@Mail_Content", model.Body);
                dp.Add("@Mail_Attachment", model.FilePath);
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

        public class MailRequest
        {
            public string Subject { get; set; }
            public string Body { get; set; }
            public string ToEmail { get; set; }
            public string CcEmail { get; set; }
            public string BccEmail { get; set; }
            public string FilePath { get; set; }
            public Stream Attachments { get; set; }
        }
    }
}