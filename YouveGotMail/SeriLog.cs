using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace YouveGotMail
{
    public class SeriLog
    {
        private static string path = ConfigurationManager.AppSettings["SeriLogsPath"];
        public static void SaveSeriLog(string action, string message)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(path + "log-.txt",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

                Log.Information("{action} [Message] {message}", action, message);

                Log.CloseAndFlush();
            }
            catch (Exception ex)
            {

            }
        }
    }
}