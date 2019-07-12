using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    class LogIt
    {
        /// <summary>
        /// Does LogIt Save the log into file
        /// </summary>
        private bool m_SaveToFile;
        /// <summary>
        /// Does LogIt Show the log onto Console
        /// </summary>
        private bool m_ShowToConsole;
        private System.Threading.ManualResetEvent m_WaitForThreadStart = new System.Threading.ManualResetEvent(false);
        static private System.Threading.AutoResetEvent m_WaitForWriteLog = new System.Threading.AutoResetEvent(true);
        /// <summary>
        /// Create LogIt
        /// </summary>
        /// <param name="f_SaveToFile">Does LogIt Save the log into file</param>
        /// <param name="f_ShowToConsole">Does LogIt Show the log onto Console</param>
        public LogIt(bool f_SaveToFile = false, bool f_ShowToConsole = false)
        {
            m_SaveToFile = f_SaveToFile;
            m_ShowToConsole = f_ShowToConsole;
        }
        static private string m_SavePath = string.Empty;
        static private Queue<string> t_LogQueue = new Queue<string>();
        static public void PushLog(string t_Log)
        {
            if (m_WaitForWriteLog.WaitOne(5000) == true)
            {
                t_LogQueue.Enqueue(t_Log);
                m_WaitForWriteLog.Set();
            }
        }
        
        public void SyncRun(string f_SavePath)
        {
            PushLog("[LogIt][SyncRun]: ++");
            m_SavePath = f_SavePath;
            System.Threading.Thread t_LogThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(LogThread));
            System.IO.File.Delete(f_SavePath);
            t_LogThread.Start(m_SavePath);
            m_WaitForThreadStart.WaitOne();
            PushLog("[LogIt][SyncRun]: --");

        }
        private void LogThread(object f_SavePath)
        {
            m_WaitForThreadStart.Set();
            string t_SavePath = f_SavePath as string;
            while(true)
            {
                while (t_LogQueue.Count != 0)
                {
                    string t_Log = t_LogQueue.Dequeue();
                    if (m_SaveToFile == true)
                    {
                        System.IO.StreamWriter t_StreamWriter = new System.IO.StreamWriter(t_SavePath, true);
                        t_StreamWriter.WriteLine(t_Log);
                        t_StreamWriter.Flush();
                        t_StreamWriter.Close();
                    }
                    if (m_ShowToConsole == true)
                    {
                        Console.WriteLine(t_Log);
                    }
                }
            }
        }
        

    }
}
