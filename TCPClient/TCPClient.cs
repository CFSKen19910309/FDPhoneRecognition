using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPClient
{
    class TCPClient
    {
        public static void Connect(String server, String message)
        {
            try
            {
                System.Threading.Thread.Sleep(1000);
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 6280;
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient(server, port);
                System.Net.Sockets.NetworkStream stream = client.GetStream();
                Byte[] t_Send = new Byte[256];
                Byte[] t_Receive = new Byte[256];
                while (true)
                {
                    message = Console.ReadLine();
                    // Translate the passed message into ASCII and store it as a Byte array.
                    t_Send = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

            
                    // Send the message to the connected TcpServer. 
                    stream.Write(t_Send, 0, t_Send.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    Array.Clear(t_Receive, 0, 256);
                    Int32 bytes = stream.Read(t_Receive, 0, t_Receive.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(t_Receive, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);

                    Thread.Sleep(10);
                }
                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
