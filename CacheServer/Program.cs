using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CacheServer
{
    class Program
    {
        //define our server socket
        private static Mutex mut = new Mutex();
        private static LimitedSizeDictionary<string, byte[]> cache = new LimitedSizeDictionary<string, byte[]>(); 
        //private static Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();
        Queue<string> queue = new Queue<string>();
        private static int port = 10011;
        private static string ip = "127.0.0.1";
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> clientSockets = new List<Socket>(); //clients list
        private static byte[] buffer = new byte[1024];
        static void Main(string[] args)
        {
            Console.Title = "Server";
            Console.ForegroundColor = ConsoleColor.Blue;
            setupServer();
            Console.ReadLine();
        }
        private static void setupServer()
        {
            Console.WriteLine("Setting up server...");
            //bind the server to listen on localhost and the defined port
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            serverSocket.Listen(100); //not really sure, backlog  - how many connections the server can accept?

            serverSocket.BeginAccept(new AsyncCallback(acceptCallBack), null);
        }
        private static void acceptCallBack(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR); //this will make the server stop accepting new clients
                                                        //so we need to call BeginAccept again in order to listen to multiple ones
            clientSockets.Add(socket);

            Console.WriteLine("Client connected...");

            socket.BeginReceive(buffer,0,buffer.Length,SocketFlags.None , new AsyncCallback(receiveCallBack) , socket);

            serverSocket.BeginAccept(new AsyncCallback(acceptCallBack), null);
        }
        private static void receiveCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            try
            {
               
                int received = socket.EndReceive(AR);

                byte[] dataBuff = new byte[received];

                Array.Copy(buffer, dataBuff, received); //write what we received from the client to our global buffer

                string text = Encoding.ASCII.GetString(dataBuff);
                Console.WriteLine("Text received : " + text);

                string response = String.Empty;

                string commandType = getCommand(text);
                if (commandType == "get")
                {
                    response = get(text);
                }
                else if (commandType == "set")
                {
                    mut.WaitOne();
                    response = set(text);
                    mut.ReleaseMutex();
                }

                byte[] data = Encoding.ASCII.GetBytes(response);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(sendCallBack), socket);

                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallBack), socket);
            }
            catch(Exception e)
            {
                Console.WriteLine("Client disconnected...");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
                clientSockets.Remove(socket);
            }
            
        }

        private static string set(string comm)
        {
            string value = String.Empty; //value
            string[] commArr = comm.Split();
            string key = commArr[1]; //key
            int size = int.Parse(commArr[2]); //size of value
            byte[] valByte = new byte[size];
            for (int i = 3; i < commArr.Length; i++) //get value
            {
                value += commArr[i];
            }
            try
            {
                
                byte[] currValByte = Encoding.ASCII.GetBytes(value);

                Array.Copy(currValByte, valByte, currValByte.Length);
                
            }
            catch(ArgumentException) // if the user inputs length and the actual input isnt the same length
            {
                return "Input length missmatched.";
            }
            try
            {
                cache.Add(key, valByte, size);
            }
            catch(Exception e)
            {
                return e.Message;
            }
            return "OK";
        }

        private static string get(string comm)
        {
            string res = String.Empty;
            string[] commArr = comm.Split();
            string key = commArr[1];
            if (cache.ContainsKey(key))
            {
                byte[] value = cache.dict[key];
                string resString = Encoding.ASCII.GetString(value);
                res = "OK " + value.Length + "\r\n" + resString;
            }
            else res = "MISSING\r\n";

            return res;
        }

        private static void sendCallBack(IAsyncResult AR) //ending the current 'chat' between client and server in order to start a new one
        {
            Socket socket = (Socket)AR.AsyncState;

            socket.EndSend(AR);
        }

        private static string getCommand(string text) //helper function to determine the command type
        {
            string[] textArr = text.Split();
            if (textArr[0] == "get")
                return "get";
            else return "set";
        }

    }
}
