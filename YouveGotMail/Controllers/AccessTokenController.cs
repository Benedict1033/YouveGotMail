using Dapper;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace YouveGotMail.Controllers
{
    [EnableCors("*", "*", "*")]

    [RoutePrefix("api")]
    public class AccessTokenController : ApiController
    {
        [HttpPost]
        [Route("accessToken")]
        public dataJWTTOKEN Post(tokenClass model)
        {
            try
            { 
                dataJWTTOKEN jwtToken = new dataJWTTOKEN();
                jwtToken = Token.GetAccessToken(model);
                Token_Log(model, "成功", "");  //存成功的Log
                return jwtToken;
            }
            catch (Exception ex)
            {
                Token_Log(model, "失敗", ex.ToString()); //存失敗的Log + exception
                throw ex;
            }
        }





        [HttpPost]
        [Route("formatTest")]
        public IHttpActionResult Test (tokenClass model)
        //public IHttpActionResult Test([FromBody]string temp)
        {
            SeriLog.SaveSeriLog("Test", "Entry: System - " + model.system);
            try
            {
                string md5 = Cryptography.ToMD5("M@kalotBPM");
                string timePW = "M@kalotBPM" + "_" + DateTime.UtcNow.Ticks; //目前預設密碼為M@kalot，正式上線後可再調整

                //編成 Base64 字串
                byte[] bytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(timePW);
                string base64PW = Convert.ToBase64String(bytes);

                string decodePW = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(model.passWord));
                string[] filtPW = decodePW.Split('_');
                DateTime dt = new DateTime(long.Parse(filtPW[1]));
                return Ok("Origin PassWord:" + filtPW[0] + ", TimeStamp:" + dt.ToString()+", Base64:"+ base64PW);
                //return Ok();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }          


        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(tokenClass model)
        {
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();

            try
            {
                //var query = "Select System FROM  YouveGotMail.dbo.System WHERE System ='" + model.system + "'";
                //SqlCommand command = new SqlCommand(query, con);
                //SqlDataReader reader = command.ExecuteReader();
                //string sys = "";
                //while (reader.Read())
                //    sys = reader["System"].ToString();

                var oeSql = @"SELECT System FROM YouveGotMail.dbo.System WHERE system = @System";
                var parameters = new DynamicParameters();
                parameters.Add("@System", model.system);
                string sys = con.Query<string>(oeSql, parameters).FirstOrDefault();

                if (sys == null || sys == "")
                {
                    var qq = "INSERT INTO YouveGotMail.dbo.System(System,Password) VALUES(@System,@Password)";
                    var dp = new DynamicParameters();
                    dp.Add("@System", model.system);
                    dp.Add("@Password", model.passWord);
                    con.Execute(qq, dp);

                    return Ok("Register success");
                }
                else
                {                    
                    return BadRequest("System name duplicated");
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message + ";" + ex.StackTrace);
            }
            
        }

        [HttpPost]
        [Route("delete")]
        public string Delete(tokenClass model)
        {
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "Select System,Password,ID FROM  YouveGotMail.dbo.System WHERE System ='" + model.system + "'";
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();
            string pass = "";
            while (reader.Read())
                pass = reader["PassWord"].ToString();

            if (model.passWord == pass)
            {
                var qq = "DELETE FROM YouveGotMail.dbo.System WHERE System ='" + model.system + "' and PassWord='" + model.passWord + "'";
                con.Execute(qq);
                return "刪除成功";
            }
            else
                return "請提供正確的密碼";
        }

        [HttpPost]
        [Route("update")]
        public string Update(tokenClass model)
        {
            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();
            var query = "Select System,Password,ID FROM  YouveGotMail.dbo.System WHERE System ='" + model.system + "'";
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();
            string ID = "";
            string pass = "";

            while (reader.Read())
            {
                ID = reader["ID"].ToString();
                pass = reader["PassWord"].ToString();
            }

            if (model.passWord == pass)
            {
                var qq = "UPDATE  YouveGotMail.dbo.System SET System =@System ,PassWord=@PassWord WHERE ID=@ID ";
                var dp = new DynamicParameters();
                dp.Add("@System", model.system);
                dp.Add("@PassWord", model.newPassWord);
                dp.Add("@ID", ID);
                con.Execute(qq, dp);
                return "修改成功";
            }
            else
                return "請提供正確的密碼";
        }

        void Token_Log(tokenClass model, string status, string ex)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            var con = new SqlConnection(ConfigurationManager.AppSettings["Connection1"]);
            con.Open();

            var query = "INSERT INTO YouveGotMail.dbo.token(Used_System,IP_Address,Access_DateTime,Access_Status,Exception) " +
                        "VALUES(@Used_System,@IP_Address,@Access_DateTime,@Access_Status,@Exception)";

            var dp = new DynamicParameters();
            dp.Add("@Used_System", model.system);
            dp.Add("@IP_Address", ip);
            dp.Add("@Access_DateTime", time);
            dp.Add("@Access_Status", status);
            dp.Add("@Exception", ex);
            con.Execute(query, dp);
        }
    }

    public class tokenClass
    {
        public string system { get; set; }
        public string passWord { get; set; }
        public string newPassWord { get; set; }
    }
}