using FreeSpaceChecker.Interfaces;
using System;
using System.IO;

namespace FreeSpaceChecker
{
    class Logger : ILogger
    {
        private string loggerPath;

        public Logger(string loggerPath)
        {
            this.loggerPath = loggerPath;
        }
        public void Log(string msg, bool isEndLine = false)
        {
            if (!Directory.Exists(Path.GetDirectoryName(loggerPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(loggerPath));

            try
            {
                string logLine = System.String.Format("{0:G}: {1}", System.DateTime.Now.ToString().PadRight(20), msg);

                Console.WriteLine(logLine);

                using (System.IO.StreamWriter sw = System.IO.File.AppendText(loggerPath))
                {
                    if(isEndLine)
                    {
                        sw.WriteLine("====================================================================================================================>");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(msg))
                            sw.WriteLine(logLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}