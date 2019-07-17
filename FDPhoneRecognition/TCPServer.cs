using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FDPhoneRecognition
{
    //Space: 0x20 (ascii code)
    //LF: 0x0A (ascii code)
    //Format: Command + [arguments] + LF
    //Format:  Response + ID + [arguments] + LF

    /// <summary>
    /// Query Phone Model Parameter 
    /// Success: ACK PMP { pmp name }, for example: “ACK PMP iphone6s RoseGold”
    /// Fail: ERR PMP { reason }, for example: “ERR PMP Recognize timeout”
    /// </summary>
    /// <param name="f_Bitmap"></param>
    /// <returns>Model Name</returns>
    delegate string QueryPMP(System.Drawing.Bitmap f_Bitmap);
    /// <summary>
    /// Query Image Sampling Parameters
    /// Success: ACK ISP { isp name }, for example: “ACK ISP iphone6s”
    /// Fail: ERR ISP { reason }, for example: “ERR ISP Recognize timeout”
    /// </summary>
    /// <returns>Model Name</returns>
    delegate string QueryISP();
    /// <summary>
    /// TCP Server Class is managed the All of Server do, 
    /// like building connection, reconnecting with client, sending/receiving data and Stop Server on
    /// </summary>
    class TCPServer
    {
        /// <summary>
        /// The ip of server
        /// </summary>
        private string m_IP;
        /// <summary>
        /// The port of server
        /// </summary>
        private int m_Port;
        /// <summary>
        /// the current statuc of Server
        /// </summary>
        private int m_ServerStatus;
        /// <summary>
        /// the server status list
        /// </summary>

        private enum E_ServerStatus
        {
            e_WaitForClientConnect,
            e_WaitForClientData,
            e_Stop,
            e_Restart,
            e_Running
        }
        /// <summary>
        /// the system ask command
        /// </summary>
        private int m_SystemStatus;
        /// <summary>
        /// the system ask command list
        /// </summary>
        private enum E_SystemStatus
        {
            e_SystemRunning,
            e_SystemStop
        }
        /// <summary>
        /// the one of all main member. it is focus on Connection building
        /// </summary>
        System.Net.Sockets.TcpListener m_TCPListener = null;
        /// <summary>
        /// the one of all main member. it save which one is connect
        /// </summary>
        System.Net.Sockets.TcpClient m_Client = null;
        /// <summary>
        /// A signal control whether a client conectting or not
        /// </summary>
        private System.Threading.AutoResetEvent m_tcpClientConnectedEvent = new System.Threading.AutoResetEvent(false);
        /// <summary>
        /// Focus on communication within client and server 
        /// </summary>
        System.Net.Sockets.NetworkStream m_Stream = null;
        
        static private string m_ColorSizeModelName = string.Empty;
        static private string m_MemoryStation = string.Empty;
        static private string m_ModelName = string.Empty;

        
        /// <summary>
        /// constuct the TCPServer
        /// </summary>
        /// <param name="f_IP">the IP of server</param>
        /// <param name="f_Port">the port of server</param>
        public TCPServer(string f_IP = "127.0.0.1", int f_Port = 5050)
        {
            LogIt.PushLog($"[TCPServer][Contruct] ++: IP = {f_IP}:{f_Port}");
            m_IP = f_IP;
            m_Port = f_Port;
            m_tcpClientConnectedEvent.Reset();
            LogIt.PushLog($"[TCPServer][Contruct] --");
        }
        /// <summary>
        /// Call the thread to run of the Server
        /// </summary>
        public void SyncRun()
        {
            LogIt.PushLog($"[TCPServer][Sync] ++");
            System.Net.IPAddress localAddr = System.Net.IPAddress.Parse(m_IP);
            m_TCPListener = new System.Net.Sockets.TcpListener(localAddr, m_Port);
            System.Threading.Thread t_Thread = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
            m_ServerStatus = (int)E_ServerStatus.e_WaitForClientConnect;
            m_SystemStatus = (int)E_SystemStatus.e_SystemRunning;
            t_Thread.Start();
            LogIt.PushLog($"[TCPServer][Contruct] --: Server Status = {m_ServerStatus}; System Status = {m_SystemStatus}");
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopRun()
        {
            LogIt.PushLog($"[TCPServer][StopRun] ++: System Status = {m_SystemStatus}");
            m_SystemStatus = (int)E_SystemStatus.e_SystemStop;
            LogIt.PushLog($"[TCPServer][StopRun] --: System Status = {m_SystemStatus}");
        }
        /// <summary>
        /// the thread region. It control all of the TCP server do.
        /// </summary>
        private void Run()
        {
            // Buffer for reading data
            LogIt.PushLog($"[TCPServer][Run] ++");
            Byte[] t_DataBytes = new Byte[256];
            try
            {
                while (m_SystemStatus != (int)E_SystemStatus.e_SystemStop)
                {
                    switch (m_ServerStatus)
                    {
                        case (int)E_ServerStatus.e_Restart:
                            {
                                LogIt.PushLog($"[TCPServer][Run][Restart] ++");
                                m_Client.Close();
                                m_TCPListener.Stop();
                                m_ServerStatus = (int)E_ServerStatus.e_WaitForClientConnect;
                                LogIt.PushLog($"[TCPServer][Run][Restart] --: Next Server Status = {m_ServerStatus}");
                                break;
                            }
                        case (int)E_ServerStatus.e_WaitForClientConnect:
                            {
                                LogIt.PushLog($"[TCPServer][Run][WaitForClientConnect] ++");
                                m_TCPListener.Start();
                                m_TCPListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTCPClientCallback), m_TCPListener);
                                
                                m_tcpClientConnectedEvent.WaitOne();
                                m_ServerStatus = (int)E_ServerStatus.e_WaitForClientData;
                                LogIt.PushLog($"[TCPServer][Run][WaitForClientConnect] --: Next  Server Status = {m_ServerStatus}");
                                break;
                            }
                        case (int)E_ServerStatus.e_WaitForClientData:
                            {
                                //LogIt.PushLog($"[TCPServer][Run][WaitForClientData] ++");
                                try
                                {
                                    bool t = m_Client.Client.Connected;
                                    int trt = m_Client.Client.Available;
                                    int t_TotalByteLength = m_Stream.Read(t_DataBytes, 0, t_DataBytes.Length);
                                    //Console.WriteLine($"Connected TotalByteLength = {t_TotalByteLength}");
                                    if (t_TotalByteLength != 0)
                                    {
                                        LogIt.PushLog($"[TCPServer][Run][WaitForClientData]: Receive Data Length = {t_DataBytes.Length}");

                                        string t_FeedBackStirng = DecisionTask(t_DataBytes, t_TotalByteLength);
                                        //m_Stream.Flush();
                                    }
                                    else
                                    {
                                        //Console.WriteLine($"Disconnect TotalByteLength = {t_TotalByteLength}"); //bool t = m_TCPListener.Server.Connected;
                                    }
                                    t_DataBytes = new byte[256];
                                }
                                catch(Exception Ex)
                                {
                                    m_ServerStatus = (int)E_ServerStatus.e_Restart;
                                }
                                //LogIt.PushLog($"[TCPServer][Run][WaitForClientData] --: Next Server Status = {m_ServerStatus}");
                                break;
                            }
                        case (int)E_ServerStatus.e_Stop:
                            {
                                LogIt.PushLog($"[TCPServer][Run][Stop] ++");
                                m_ServerStatus = (int)E_ServerStatus.e_Stop;
                                LogIt.PushLog($"[TCPServer][Run][Stop] --: Server Status = {m_ServerStatus}");
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                LogIt.PushLog($"[TCPServer][Run]: SocketException={e.Message}");
            }
            finally
            {
                m_Stream.Close();
                m_Client.Close();
                m_TCPListener.Stop();
            }
            LogIt.PushLog($"[TCPServer][Run] ++");
        }
        /// <summary>
        /// when Server Receive the Data from client, it will decide which task will do
        /// </summary>
        /// <param name="f_DataBytes">The data content</param>
        /// <param name="f_DataLength">The data length</param>
        /// <returns></returns>
        private string DecisionTask(byte[] f_DataBytes, int f_DataLength)
        {
            
            // Translate data bytes to a ASCII string.
            LogIt.PushLog($"[TCPServer][DecisionTask] ++");
            string t_Data = System.Text.Encoding.ASCII.GetString(f_DataBytes, 0, f_DataLength);
            string t_Feedback =string.Empty;
            LogIt.PushLog($"[TCPServer][DecisionTask]: Received Data = {t_Data}");
            if (t_Data.IndexOf("QueryISP") >= 0)
            {
                //Mapping table when get size and color
                //Start Chris Color and Size Detect
                m_ColorSizeModelName = "Iphone6s Gray";
                if(string.IsNullOrEmpty(m_ColorSizeModelName) == true)
                {
                    t_Feedback = $"ERR ISP UnSuccessful";
                }
                else
                {
                    t_Feedback = $"ACK ISP {m_ColorSizeModelName}{System.Text.Encoding.ASCII.GetString(new byte[] { 0x0A })}";
                }
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(t_Feedback);
                m_Stream.Write(msg, 0, msg.Length);
            }
            if(t_Data.IndexOf("QueryPMP") >= 0)
            {
                //Get modle name
                m_ModelName = "Iphone6 Gray";
                if (string.IsNullOrEmpty(m_ModelName) == true)
                {
                    t_Feedback = $"ERR PMP UnSuccessful";
                }
                else
                {
                    t_Feedback = $"ACK PMP {m_ModelName}{System.Text.Encoding.ASCII.GetString(new byte[] { 0x0A })}";
                }
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(t_Feedback);
                m_Stream.Write(msg, 0, msg.Length);
            }
            if(t_Data.IndexOf("MMI") >=0)
            {
                m_MemoryStation = t_Data.Substring(3).Trim();
                t_Feedback = $"ACK MMI {m_MemoryStation}{System.Text.Encoding.ASCII.GetString(new byte[] { 0x0A })}";

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(t_Feedback);
                m_Stream.Write(msg, 0, msg.Length);

                ShareMemory t_SharedMemory = new ShareMemory(m_MemoryStation);
                t_SharedMemory.SyncGetMemory();

                //Start Chris Model Detect
            }
            // Process the data sent by the client.
            LogIt.PushLog($"[TCPServer][DecisionTask]: Send Data = {t_Feedback}");
            LogIt.PushLog($"[TCPServer][DecisionTask] --");
            return string.Empty ;
        }
        /// <summary>
        /// This is callback when someone connected with server
        /// </summary>
        /// <param name="f_IAsyncResult">the interface from System.Net.Sockets.TcpListener</param>
        private void DoAcceptTCPClientCallback(IAsyncResult f_IAsyncResult)
        {
            LogIt.PushLog($"[TCPServer][DoAcceptTCPClientCallback] ++");
            m_TCPListener = (System.Net.Sockets.TcpListener)f_IAsyncResult.AsyncState;
            m_Client = m_TCPListener.EndAcceptTcpClient(f_IAsyncResult);
            m_Stream = m_Client.GetStream();
            m_tcpClientConnectedEvent.Set();
            LogIt.PushLog($"[TCPServer][DoAcceptTCPClientCallback] --");
        }

        #region static functions
        public static void start(System.Threading.EventWaitHandle quitEvent, System.Collections.Specialized.StringDictionary args)
        {
            Program.logIt("TCPServer::start: ++");
            int t_Port = 6280;
            if (args.ContainsKey("port"))
            {
                if(!Int32.TryParse(args["port"], out t_Port))
                {
                    t_Port = 6280;
                }
            }
            try
            {
                TcpListener server = new TcpListener(IPAddress.Loopback, t_Port);
                server.Start();
                server.BeginAcceptSocket(new AsyncCallback(DoAcceptClientCallback), server);

                Console.WriteLine("Server is started. press any key to terminate.");
                while (!quitEvent.WaitOne(1000))
                {
                    if (Console.KeyAvailable)
                    {
                        Program.logIt("TCPServer::start: is going to be terminated by user key input.");
                        break;
                    }
                }
            }
            catch (Exception) { }
            Program.logIt("TCPServer::start: --");
        }
        static void DoAcceptClientCallback(IAsyncResult ar)
        {
            Program.logIt("DoAcceptTcpClientCallback: ++");
            try
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                Socket client = listener.EndAcceptSocket(ar);
                new TaskFactory().StartNew(new Action<object>((c) =>
                {
                    handle_client((Socket)c);
                }), client);
                listener.BeginAcceptSocket(new AsyncCallback(DoAcceptClientCallback), listener);
            }
            catch (Exception) { }
            Program.logIt("DoAcceptTcpClientCallback: --");
        }
        static void handle_client(Socket client)
        {
            Program.logIt("handle_client: ++");
            MemoryStream ms = new MemoryStream();
            Dictionary<string, object> current_task = null;
            try
            {
                while (client.Connected)
                {
                    if(client.Connected && client.Available>0)
                    {
                        byte[] buf = new byte[client.Available];
                        int r = client.Receive(buf);
                        ms.Write(buf, 0, r);
                    }
                    // check if incoming data complete, 
                    // Command is terminated by LF (line feed) (0x0a)
                    if (ms.Length > 0)
                    {
                        byte[] data = ms.ToArray();
                        if (data.Last() == 0x0a)
                        {
                            string cmd = System.Text.Encoding.UTF8.GetString(data);
                            ms.SetLength(0);
                            Program.logIt($"handle_client: recv command: {cmd}");
                            Tuple<int, string> res = handle_command(cmd.Split(null), ref current_task);
                            if (res.Item1==0)
                            {
                                // task started success.
                            }
                            else if (res.Item1 == 3)
                            {
                                // task cancelled
                                string s = $"ERR {res.Item2} Abort{System.Environment.NewLine}";
                                client.Send(System.Text.Encoding.UTF8.GetBytes(s));
                            }
                            else
                            {
                                // error,
                                string s = $"ERR  ";
                            }
                        }
                    }
                    if (current_task != null)
                    {
                        object o;
                        if(current_task.TryGetValue("task", out o))
                        {
                            Task<Dictionary<string, object>> t = (Task<Dictionary<string, object>>)o;
                            if (t.IsCompleted)
                            {
                                // completed, get result
                                Tuple<bool, string> res = handle_command_complete(current_task);
                                if(res.Item1)
                                    client.Send(System.Text.Encoding.UTF8.GetBytes(res.Item2));
                            }
                            else if (t.IsCanceled)
                            {
                                // cancelled
                            }
                            else if (t.IsFaulted)
                            {
                                // unhandled exception
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception) { }
            Program.logIt("handle_client: --");
        }
        static Tuple<bool, string> handle_command_complete(Dictionary<string,object> args)
        {
            object o;
            string tid=string.Empty;
            bool ret = false;
            string response = string.Empty;
            if (args.TryGetValue("id", out o))
                tid = o as string;
            if (string.Compare(tid, "Load", true) == 0)
            {
                // prepare response for command QueryLoad
            }
            return new Tuple<bool, string>(ret, response);
        }
        static Tuple<int,string> handle_command(string[] cmds, ref Dictionary<string,object> current_task)
        {
            int error = -1;
            string id = string.Empty;
            Program.logIt($"handle_command: ++ {string.Join(" ", cmds)}");
            if (string.Compare(cmds[0], "Abort", true) == 0)
            {
                if (current_task == null)
                {
                    Program.logIt($"handle_command: no current task can be aborted");
                    error = 2;
                }
                else
                {
                    try
                    {
                        object o;
                        if (current_task.TryGetValue("CancellationTokenSource", out o))
                        {
                            CancellationTokenSource cts = (CancellationTokenSource)o;
                            cts.Cancel();
                            if (current_task.TryGetValue("id", out o))
                                id = o as string;
                            error = 3;
                        }
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                if (current_task != null)
                {
                    Program.logIt($"handle_command: current task is still running. Cannot start another command.");
                    error = 1;
                }
                else
                {
                    if (string.Compare(cmds[0], "QueryISP", true) == 0)
                    {
                        // wait for QueryISP.
                        // return only when 1) QueryISP return, or 2) abort called
                        id = "ISP";
                        var tokenSource = new CancellationTokenSource();
                        Task<Dictionary<string, object>> t = Task.Factory.StartNew((o) =>
                        {
                            Dictionary<string, object> ret = new Dictionary<string, object>();
                            CancellationToken ct = (CancellationToken)o;
                            while (true)
                            {
                                System.Threading.Thread.Sleep(1000);
                                if (ct.IsCancellationRequested)
                                {
                                    Program.logIt($"handle_command: QueryISP cancelled.");
                                    break;
                                }
                            }
                            return ret;
                        }, tokenSource.Token);
                        error = 0;
                        current_task = new Dictionary<string, object>();
                        current_task.Add("CancellationTokenSource", tokenSource);
                        current_task.Add("task", t);
                        current_task.Add("id", id);
                        current_task.Add("starttime", DateTime.Now);
                    }
                    else if (string.Compare(cmds[0], "QueryPMP", true) == 0)
                    {
                        // wait for QueryPMP.
                        // return only when 1) QueryPMP return, or 2) abort called
                        id = "PMP";
                        var tokenSource = new CancellationTokenSource();
                        Task<Dictionary<string, object>> t = Task.Factory.StartNew((o) =>
                        {
                            Dictionary<string, object> ret = new Dictionary<string, object>();
                            CancellationToken ct = (CancellationToken)o;
                            while (true)
                            {
                                System.Threading.Thread.Sleep(1000);
                                if (ct.IsCancellationRequested)
                                {
                                    Program.logIt($"handle_command: QueryISP cancelled.");
                                    break;
                                }
                            }
                            return ret;
                        }, tokenSource.Token);
                        error = 0;
                        current_task = new Dictionary<string, object>();
                        current_task.Add("CancellationTokenSource", tokenSource);
                        current_task.Add("task", t);
                        current_task.Add("id", id);
                        current_task.Add("starttime", DateTime.Now);
                    }
                    else if (string.Compare(cmds[0], "MMI", true) == 0)
                    {

                    }
                    else if (string.Compare(cmds[0], "QueryLoad", true) == 0)
                    {
                        // wait for user load the device.
                        // return only when 1) device loaded, or 2) abort called
                        id = "Load";
                        var tokenSource = new CancellationTokenSource();
                        Task<Dictionary<string, object>> t = Task.Factory.StartNew((o) =>
                        {
                            Dictionary<string, object> ret = new Dictionary<string, object>();
                            CancellationToken ct = (CancellationToken)o;
                            while (true)
                            {
                                System.Threading.Thread.Sleep(1000);
                                if (ct.IsCancellationRequested)
                                {
                                    Program.logIt($"handle_command: QueryLoad cancelled.");
                                    break;
                                }
                            }
                            return ret;
                        }, tokenSource.Token);
                        error = 0;
                        current_task = new Dictionary<string, object>();
                        current_task.Add("CancellationTokenSource", tokenSource);
                        current_task.Add("task", t);
                        current_task.Add("id", id);
                        current_task.Add("starttime", DateTime.Now);
                    }
                    else if (string.Compare(cmds[0], "QueryUnload", true) == 0)
                    {
                        // wait for user unload the device.
                        // return only when 1) device loaded, or 2) abort called
                        id = "Unload";
                        var tokenSource = new CancellationTokenSource();
                        Task<Dictionary<string, object>> t = Task.Factory.StartNew((o) =>
                        {
                            Dictionary<string, object> ret = new Dictionary<string, object>();
                            CancellationToken ct = (CancellationToken)o;
                            while (true)
                            {
                                System.Threading.Thread.Sleep(1000);
                                if (ct.IsCancellationRequested)
                                {
                                    Program.logIt($"handle_command: QueryUnload cancelled.");
                                    break;
                                }
                            }
                            return ret;
                        }, tokenSource.Token);
                        error = 0;
                        current_task = new Dictionary<string, object>();
                        current_task.Add("CancellationTokenSource", tokenSource);
                        current_task.Add("task", t);
                        current_task.Add("id", id);
                        current_task.Add("starttime", DateTime.Now);
                    }
                    else
                    {

                    }
                }
            }
            Program.logIt($"handle_command: -- ret={error}");
            return new Tuple<int, string>(error,id);
        }
        #endregion
    }
}
