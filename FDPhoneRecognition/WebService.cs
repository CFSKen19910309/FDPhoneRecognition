using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    /// <summary>
    /// Define the WebService provide what kind of the service
    /// </summary>
    [ServiceContract]
    public interface IWebService
    {
        /// <summary>
        /// Get the color and size of phone
        /// </summary>
        /// <returns>Model Name</returns>
        [OperationContract]
        [WebGet(UriTemplate = "GetModelOfPhoneSizeAndColor")]
        string GetModelOfPhoneSizeAndColor();
        /// <summary>
        /// Get the model name from the Back Image Data
        /// </summary>
        /// <param name="f_ImagePath">the image path send by machine sortware.</param>
        /// <returns>Model Name</returns>
        [OperationContract]
        [WebGet(UriTemplate = "GetFinalModel?ImagePath={f_ImagePath}")]
        string GetFinalModel(string f_ImagePath);
    }
    class WebService : IWebService
    {
        /// <summary>
        /// Get the color and size of phone
        /// </summary>
        /// <returns>Model Name</returns>
        public string GetModelOfPhoneSizeAndColor()
        {
            return "IPhone6s Plus Gold";
        }
        /// <summary>
        /// Get the model name from the Back Image Data
        /// </summary>
        /// <param name="f_ImagePath">the image path send by machine sortware.</param>
        /// <returns>Model Name</returns>
        public string GetFinalModel(string f_ImagePath)
        {
            return "IPhone7s Plus Gold";
        }
        
        public static void run(System.Threading.EventWaitHandle quitEvent, System.Collections.Specialized.StringDictionary args)
        {
            try
            {
                int port = 21173;
                if (args.ContainsKey("port"))
                {
                    if (!Int32.TryParse(args["port"], out port))
                    {
                        port = 21173;
                    }
                }
                string s = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System), "netsh.exe");
                if (System.IO.File.Exists(s))
                {
                    int i;
                    string param = string.Format("http add urlacl url=http://+:{0}/ user=Everyone", port);
                    string[] lines = runExe(s, param, out i, null);
                }

                Uri t_BaseAddress = new Uri(string.Format("http://localhost:{0}", port));
                WebServiceHost t_WebServiceHost = new WebServiceHost(typeof(WebService), t_BaseAddress);
                t_WebServiceHost.Open();
                quitEvent.WaitOne();
                t_WebServiceHost.Close();
                DateTime t_StartTime = DateTime.Now;
                while (t_WebServiceHost.State != CommunicationState.Closed)
                {
                    System.Threading.Thread.Sleep(1000);
                    if ((DateTime.Now - t_StartTime).TotalSeconds > 30)
                    {
                        break;
                    }
                }
            }
            catch (Exception Ex)
            {

            }
        }
        public static string[] runExe(string exeFilename, string param, out int exitCode, System.Collections.Specialized.StringDictionary env, int timeout = 60 * 1000)
        {
            List<string> ret = new List<string>();
            exitCode = 1;
            try
            {
                if (System.IO.File.Exists(exeFilename))
                {
                    System.Threading.AutoResetEvent ev = new System.Threading.AutoResetEvent(false);
                    Process p = new Process();
                    p.StartInfo.FileName = exeFilename;
                    p.StartInfo.Arguments = param;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.CreateNoWindow = true;
                    if (env != null && env.Count > 0)
                    {
                        foreach (DictionaryEntry de in env)
                        {
                            p.StartInfo.EnvironmentVariables.Add(de.Key as string, de.Value as string);
                        }
                    }
                    p.OutputDataReceived += (obj, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                        {
                            ret.Add(args.Data);
                        }
                        if (args.Data == null)
                            ev.Set();
                    };
                    p.Start();
                    p.BeginOutputReadLine();
                    if (p.WaitForExit(timeout))
                    {
                        ev.WaitOne(timeout);
                        if (!p.HasExited)
                        {
                            exitCode = 1460;
                            p.Kill();
                        }
                        else
                            exitCode = p.ExitCode;
                    }
                    else
                    {
                        if (!p.HasExited)
                        {
                            p.Kill();
                        }
                        exitCode = 1460;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return ret.ToArray();
        }
    }
}
