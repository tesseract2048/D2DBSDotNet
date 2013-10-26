using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace D2DBSDotNet
{
    class DBSLog
    {
        StreamWriter LogWriter;
        List<string> LogLevels;
        object LogLock;

        public DBSLog()
        {
            LogLock = new object();
            try
            {
                LogWriter = new StreamWriter(D2DBS.config["logfile"], true, System.Text.Encoding.UTF8);
                LogLevels = new List<string>(D2DBS.config["loglevels"].Split(','));
                Write("info", "DBSLog Init.");
            }
            catch (Exception e)
            {
                Write("fatal", "Cannot open log file: " + e.Message);
                D2DBS.Cleanup();
            }
        }
        public void Write(string Level, string Content)
        {
            if(!LogLevels.Contains(Level)) return;
            StackTrace stackTrace = new StackTrace();
            lock(LogLock)
            {
                LogWriter.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss") + "." + DateTime.Now.Millisecond.ToString("D3") + " [" + Level + "] " + stackTrace.GetFrame(1).GetMethod().Name + ": " + Content);
                LogWriter.Flush();
            }
        }
    }
}
