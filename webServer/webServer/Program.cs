using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace webServer
{
    class Program
    {
        static TcpListener tcpListener;
        static ServerObject server;
        const int Port = 5505;
        static void Main(string[] args)
        {
            try
            {
                tcpListener = new TcpListener(/*IPAddress.Parse("127.0.0.1")*/IPAddress.Loopback, Port);
                server = new ServerObject(tcpListener);
                Thread serverThread = new Thread(new ThreadStart(server.Process));
                serverThread.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (tcpListener != null)
                    tcpListener.Stop();
            }
            /*using (var md5Hash = MD5.Create()) {
                var passwdHash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                users.Add(new User(BitConverter.ToString(passwdHash).Replace("-", String.Empty), login));
            }*/
        }
    }
}
