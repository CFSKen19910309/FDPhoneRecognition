using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    }
}
