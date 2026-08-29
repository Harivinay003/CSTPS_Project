using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLogging
{
    public static class Log
    {

        public static void WriteLog(string message)
        {
            if(Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Logs") == false)
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Logs");
            }   
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\\\Logs\\DataLogging.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
