using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            Console.Title = "Client";
            Console.ForegroundColor = ConsoleColor.Green;
            loopConnect(); //connect to server
            sendLoop();

            Console.ReadLine();
        }

        private static void sendLoop()
        {
            try
            {
                while (IsSocketConnected(clientSocket))
                {
                    Console.Write("Enter a request :");
                    string req = Console.ReadLine();
                    byte[] buff = Encoding.ASCII.GetBytes(req);
                    clientSocket.Send(buff);

                    byte[] recievedBuff = new byte[1024];

                    int rec = clientSocket.Receive(recievedBuff);

                    byte[] data = new byte[rec];
                    Array.Copy(recievedBuff, data, rec);

                    Console.WriteLine("Recieved from server: \r\n" + Encoding.ASCII.GetString(data));
                }
            }
            catch (Exception)
            {
                exit();
            }
            finally
            {
                exit();
            }
        }

        private static void loopConnect()
        {
            int attemps = 0;

            while (!clientSocket.Connected)
            {
                try
                {
                    clientSocket.Connect(IPAddress.Loopback, 10011);
                    attemps++;

                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Failed to connect " + attemps + " times...");
                }
            }
            Console.Clear();
            Console.WriteLine("client connected...");

        }
        static bool IsSocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }
        private static void exit()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            Environment.Exit(0);
        }
    }
}
