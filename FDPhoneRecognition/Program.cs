using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace FDPhoneRecognition
{
    class Program
    {
        private static readonly log4net.ILog m_Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string t_EventName = "FDPhoneRecognitionEvent";
        public static string t_StopEventName = "FDPhoneRecognitionServerEvent";
        public static void logIt(String msg)
        {
            System.Diagnostics.Trace.WriteLine(msg);
        }
        static void Main(string[] args)
        {
            string t_ArgsString = string.Empty;
            foreach (string t_Args in args)
            {
                t_ArgsString += t_Args + " ";
                
            }
            m_Log.Info($"[Main] ++: args = {t_ArgsString}");
            System.Configuration.Install.InstallContext t_args = new System.Configuration.Install.InstallContext(null, args);
            if (t_args.IsParameterTrue("debug"))
            {
                m_Log.Debug($"[Main][Debug] ++");
                m_Log.Info("[Main][Debug]: Wait for debug, press any key to continue...");
                System.Console.ReadKey();
                m_Log.Debug($"[Main][Debug] --");
            }
            if (t_args.IsParameterTrue("Start-TCPServer"))
            {
                m_Log.Debug($"[Main][Start-TCPServer] ++");
                bool own;
                System.Threading.EventWaitHandle e = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset, t_StopEventName, out own);
                m_Log.Debug($"[Main][Start-TCPServer]: EventName = {t_StopEventName}; IsNewCreated = {own.ToString()}");
                if (own)
                {
                    //Task t = Task.Run(() => TCPServer.start(e));
                    TCPServer.start(e, t_args.Parameters);
                }
                else
                {
                    // server already running. just quit
                }
                m_Log.Debug($"[Main][Start-TCPServer] --");
            }
            else if (t_args.IsParameterTrue("Kill-TCPServer"))
            {
                m_Log.Debug($"[Main][Kill-TCPServer] ++");
                try
                {
                    System.Threading.EventWaitHandle t_TCPQuit = System.Threading.EventWaitHandle.OpenExisting(t_StopEventName);
                    t_TCPQuit.Set();
                }
                catch (Exception) { }
                m_Log.Debug($"[Main][Kill-TCPServer] --");
            }
            else
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("one", 1);
                dic.Add("two", 2);
                int i = (int)dic["one"];
                int j = (int)dic?["zero"];
            }
            m_Log.Info($"[Main] --");

        }
        static void Main_1(string[] args)
        {
            //Start Log
            LogIt m_Log = new LogIt(false, true);
            m_Log.SyncRun($@"{System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Log.txt");
            //Get the input parameters
            System.Configuration.Install.InstallContext t_args = new System.Configuration.Install.InstallContext(null, args);
            //debugging waiting
            if(t_args.IsParameterTrue("debug") == true)
            {
                LogIt.PushLog("[Program][debug] ++");
                System.Console.WriteLine("Wait for debug, press any key to continue...");
                System.Console.ReadKey();
                LogIt.PushLog("[Program][debug] --");
            }
            //Start-TCPServer
            if (t_args.IsParameterTrue("Start-TCPServer") == true)
            {
                LogIt.PushLog("[Program][Start-TCPServer] ++");
                string t_IP = "127.0.0.1";
                int t_Port = 6280;
                if(t_args.IsParameterTrue("IP") == true)
                {
                    t_IP = t_args.Parameters["IP"];
                }
                if(t_args.IsParameterTrue("Port") == true)
                {
                    t_Port = Convert.ToInt32(t_args.Parameters["Port"]);
                }
                bool t_CreateNew;
                System.Threading.EventWaitHandle t_TCPQuit = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset, t_StopEventName, out t_CreateNew);
                if (t_CreateNew == true)
                {
                    TCPServer t_TCPServer = new TCPServer(t_IP, t_Port);
                    t_TCPServer.SyncRun();
                }
                else
                {
                    t_TCPQuit.Close();
                }
                LogIt.PushLog("[Program][Start-TCPServer] --");
            }
            //Kill-TCPServer
            if (t_args.IsParameterTrue("Kill-TCPServer"))
            {
                LogIt.PushLog("[Program][Kill-TCPServer] ++");
                System.Threading.EventWaitHandle t_TCPQuit = System.Threading.EventWaitHandle.OpenExisting(t_StopEventName);
                t_TCPQuit.Set();

                LogIt.PushLog("[Program][Kill-TCPServer] --");
            }
            //Start-WebService
            if (t_args.IsParameterTrue("Start-WebService"))
            {
                LogIt.PushLog("[Program][Start-WebService] ++");

                bool own;
                System.Threading.EventWaitHandle t_WebQuit = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset, t_EventName, out own);
                if (!own)
                {
                    t_WebQuit.Close();
                }
                else
                {
                    start(t_WebQuit, t_args.Parameters);
                }
                LogIt.PushLog("[Program][Start-WebService] --");
            }
            //Kill-WebService
            if (t_args.IsParameterTrue("Kill-WebService"))
            {
                LogIt.PushLog("[Program][Kill-WebService] ++");
                try
                {
                    System.Threading.EventWaitHandle t_WebQuit = System.Threading.EventWaitHandle.OpenExisting(t_EventName);
                    t_WebQuit.Set();
                }
                catch (Exception Ex)
                {
                    
                }
                LogIt.PushLog("[Program][Kill-WebService] --");
            }
            bool t_Own;
            System.Threading.EventWaitHandle t_Quit = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset, t_StopEventName, out t_Own);
            while (true)
            {
                if (t_Quit.WaitOne(5000) == true)
                {
                    //Close
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }

        }
        public static void start(System.Threading.EventWaitHandle f_Quit, System.Collections.Specialized.StringDictionary f_Args)
        {
            System.Threading.Thread t_ThreadWebService = new System.Threading.Thread(() =>
               {
                   WebService.run(f_Quit, f_Args);
               });
            t_ThreadWebService.IsBackground = true;
            t_ThreadWebService.Name = "WebService";
            t_ThreadWebService.Start();
            while(!f_Quit.WaitOne(1000))
            {
                if(System.Console.KeyAvailable)
                {
                    f_Quit.Set();
                }
            }
        }
      
    }
    
}
