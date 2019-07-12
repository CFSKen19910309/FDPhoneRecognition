using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    class QueryImageSampling
    {
        /// <summary>
        /// The file path of execution file
        /// </summary>
        private string m_ExePath = "";
        /// <summary>
        /// The file path of execution file 
        /// </summary>
        public string ExePath
        {
            get
            {
                return m_ExePath;
            }
            set
            {
                m_ExePath = value;
            }
        }
        private System.Threading.AutoResetEvent m_ReceivedDataEvent = new System.Threading.AutoResetEvent(false);
        /// <summary>
        /// the Data from Execution file
        /// </summary>
        private List<string> m_ReceiveData = new List<string>();
        /// <summary>
        /// the query image sampling contructure
        /// </summary>
        public QueryImageSampling(string f_ExePath)
        {
            ExePath = f_ExePath;
        }
        /// <summary>
        /// Wait for Execution file exit and finish max time
        /// </summary>
        private int m_Timeout = 10000;
        public string DoQueryImageSampling()
        {
            string t_ModelName = string.Empty;
            Dictionary<string, string> t_ReceiveData = LaunchApp("");//LaunchApp -> Get Color and Size or Model Index
            if(t_ReceiveData.Count == 0)
            {
                t_ModelName = "None";
                return t_ModelName;
            }
            //Look up table to model
            
            return t_ModelName;
        }
        private Dictionary<string, string> LaunchApp(string f_FilePath)
        {

            Dictionary<string, string> t_StdIn = new Dictionary<string, string>();

            System.Diagnostics.Process t_ISPProcess = new System.Diagnostics.Process();
            t_ISPProcess.StartInfo.FileName = f_FilePath;
            t_ISPProcess.StartInfo.Arguments = "";
            t_ISPProcess.StartInfo.UseShellExecute = false;
            t_ISPProcess.StartInfo.RedirectStandardOutput = true;
            t_ISPProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            t_ISPProcess.StartInfo.CreateNoWindow = true;
            t_ISPProcess.Start();

            t_ISPProcess.OutputDataReceived += DataReceived;
            t_ISPProcess.BeginOutputReadLine();

            if(t_ISPProcess.WaitForExit(m_Timeout))
            {
                m_ReceivedDataEvent.WaitOne(m_Timeout);
                if(t_ISPProcess.HasExited == true)
                {
                    t_ISPProcess.Kill();
                }
                else
                {
                    if(t_ISPProcess.ExitCode == 1)
                    {
                        t_StdIn = ParsingReceiveData(m_ReceiveData);
                        return t_StdIn;
                    }
                }
            }
            return null;
        }
        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if(string.IsNullOrEmpty(e.Data) == false)
            {
                m_ReceiveData.Add(e.Data);
            }
            if(e.Data == null)
            {
                m_ReceivedDataEvent.Set();
            }
        }
        private Dictionary<string, string> ParsingReceiveData(List<string> f_ReceiveData)
        {
            Dictionary<string, string> t_Dic = new Dictionary<string, string>();
            foreach(string t_Data in f_ReceiveData)
            {
                string[] t_KeyValue = t_Data.Split('=');
                t_Dic.Add(t_KeyValue[0], t_KeyValue[1]);
            }
            return t_Dic;
        }

    }
}
