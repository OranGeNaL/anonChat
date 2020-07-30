using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace webServer
{
    class ServerObject
    {
        TcpListener tcpListener;
        List<ClientObject> onlineUsers = new List<ClientObject>();

        public ServerObject(TcpListener listener)
        {
            tcpListener = listener;
        }

        public void Process()
        {
            Console.WriteLine("Сервер запущен, ожидание подключений...");

            while (true)
            {
                tcpListener.Start();
                TcpClient client = tcpListener.AcceptTcpClient();
                ClientObject clientObject = new ClientObject(client, this);

                Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                clientThread.Start();
            }
        }

        public void AddOnlineUser(ClientObject client)
        {
            onlineUsers.Add(client);
        }

        public void RemoveOnlineUser(ClientObject client)
        {
            onlineUsers.Remove(client);
        }

        public void SendMessageToAll(string message, ClientObject sender)
        {
            foreach(var i in onlineUsers)
            {
                if(i != sender)
                {
                    i.WriteToStream(message);
                }
            }
        }
    }
}
