using System;
using Opgave_1_Bog;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Echoserverconcurrent101
{
    public class Program
    {



        static void Main(string[] args)
        {
            int port = 4646;
            int clientNr = 0;

            Console.WriteLine("Hello Echo Server!");

            IPAddress ip = GetIp();
            TcpListener ServerListener = StartServer(ip, port);

            do
            {
                TcpClient ClientConnection = GetConnectionSocket(ServerListener, ref clientNr);
                Task.Run(() => ReadWriteStream(ClientConnection, ref clientNr));

            } while (clientNr != 0);

            StopServer(ServerListener);
        }

        private static void StopServer(TcpListener serverListener)
        {
            serverListener.Stop();
            Console.WriteLine("listener stopped");
        }

        private static TcpClient GetConnectionSocket(TcpListener serverListener, ref int clientNr)
        {

            TcpClient connectionSocket = serverListener.AcceptTcpClient();
            clientNr++;
            Console.WriteLine("Client " + clientNr + " connected");
            return connectionSocket;
        }

        private static void ReadWriteStream(TcpClient connectionSocket, ref int clientNr)
        {
            List<Bog> Book = new List<Bog>()
        {
            new Bog("Hajens vej til Danmark", "Ulrikke Holt", 365, "HH12345678912"),
            new Bog("Herrens Veje", "Åge Nielsen", 678, "ILJ4567891234")
        };

            Stream ns = connectionSocket.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);
            sw.AutoFlush = true; 

            string message = sr.ReadLine();
            Thread.Sleep(1000);
            string answer = "";
            while (message != null && message != "")
            {
                Console.WriteLine("Client: " + clientNr + " " + message);
                string[] messageArray = message.Split(' ');
                string subMessage = message.Substring(message.IndexOf(' ') + 1);
                Thread.Sleep(1000);

                switch (messageArray[0])
                {
                    case "GetAll":
                        sw.WriteLine("Get All Received");
                        sw.WriteLine(JsonConvert.SerializeObject(Book));
                        break;
                    case "Get":
                        sw.WriteLine("Get Reveived");
                        sw.WriteLine(JsonConvert.SerializeObject(Book.Find(i => i.Isbn13 == subMessage)));
                        break;
                    case "Save":
                        sw.WriteLine("Saved");
                        Bog saveBook = JsonConvert.DeserializeObject<Bog>(subMessage);
                        Book.Add(saveBook);
                        break; ;
                }

                Console.WriteLine("Client: " + message);
                sw.WriteLine(answer);
                message = sr.ReadLine();
            }

            Console.WriteLine("Empty message detected");
            ns.Close();
            connectionSocket.Close();
            clientNr--;
            Console.WriteLine("connection socket " + clientNr + " closed");

        }

        private static TcpListener StartServer(IPAddress ip, int port)
        {
            TcpListener serverSocket = new TcpListener(ip, port);
            serverSocket.Start();

            Console.WriteLine("server started waiting for connection!");

            return serverSocket;
        }

        private static IPAddress GetIp()
        {
            string name = "google.com";
            IPAddress[] addrs = Dns.GetHostEntry(name).AddressList;
            Console.WriteLine("Google IP returned by GetHostEntry" + addrs[0]);

            IPAddress ip = IPAddress.Parse("192.168.87.103");
            Console.WriteLine("Local host IP:" + ip);
            return ip;
        }

    }
}
