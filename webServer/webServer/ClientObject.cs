using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace webServer
{
    class ClientObject
    {
        public ServerObject server;
        private TcpClient tcpClient;
        private NetworkStream stream = null;
        private BinaryReader reader = null;
        private BinaryWriter writer = null;

        User user = null;

        public ClientObject(TcpClient client, ServerObject serverObj)
        {
            tcpClient = client;
            server = serverObj;
        }

        public void Process()
        {
            try
            {
                stream = tcpClient.GetStream();
                reader = new BinaryReader(stream);
                writer = new BinaryWriter(stream);
                Thread getRequestThread = new Thread(new ThreadStart(GetRequest));
                getRequestThread.Start();

            }
            catch(Exception ex)
            {
                CloseClient();
                Console.WriteLine(ex.Message);
            }
        }

        private void GetRequest()
        {
            try
            {
                while (true)
                {
                    string request = reader.ReadString();
                    if (request == Request.REGISTR)
                    {
                        Registry();
                        CloseClient();
                        break;
                    }
                    if (request == Request.LOGIN)
                        Login();
                }
            }
            catch
            {
                if (user != null)
                {
                    string message = String.Format("{0} : Пользователь {1} вышел из сети.", DateTime.Now.ToShortTimeString(), user.login);
                    Console.WriteLine(message);
                    server.SendMessageToAll(message, this);
                }
                CloseClient();
            }
            
        }

        private void MessageGet()
        {
            try
            {
                while (true)
                {
                    string message = reader.ReadString();
                    message = String.Format("{0} : {1} : {2}", DateTime.Now.ToShortTimeString(), user.login, message);
                    server.SendMessageToAll(message, this);
                }
            }
            catch
            {

            }
        }

        private void Login()
        {
            WriteToStream(Request.OK);
            string login = GetReaderStream().ReadString();
            int uIndex = -1;

            for(int i = 0; i < Storage.users.Count; i++)
            {
                if(Storage.users[i].login == login)
                {
                    uIndex = i;
                    break;
                }
            }

            if(uIndex != -1)
            {
                WriteToStream(Request.OK);
                string password = GetReaderStream().ReadString();
                if(password == Storage.users[uIndex].password)
                {
                    WriteToStream(Request.OK);
                    user = Storage.users[uIndex];
                    Console.WriteLine("{0} : Успешная авторизация пользователя {1}.", DateTime.Now.ToShortTimeString(), user.login);
                    server.AddOnlineUser(this);
                    server.SendMessageToAll(String.Format("{0} : Пользователь {1} вошел в сеть.", DateTime.Now.ToShortTimeString(), Storage.users[uIndex].login), this);
                    user.online = true;
                    MessageGet();
                }
                else
                {
                    WriteToStream(Request.INVALID_PASSWORD);
                    Console.WriteLine("{0} : Неудачная авторизация пользователя {1}. Неверный пароль.", DateTime.Now.ToShortTimeString(), user.login);
                }
            }
            else
            {
                WriteToStream(Request.USER_DOES_NOT_EXIST);
            }

            writer.Flush();
        }

        private void Registry()
        {
            WriteToStream(Request.OK);
            string login = GetReaderStream().ReadString();
            string password = GetReaderStream().ReadString();
            bool alreadyExist = false;

            foreach(var i in Storage.users)
            {
                if (i.login == login)
                {
                    alreadyExist = true;
                    break;
                }
            }
            if (!alreadyExist)
            {
                User user = new User(login, password);
                Console.WriteLine("{0} : Зарегестрирован новый пользователь {1}.", DateTime.Now.ToShortTimeString(), login);
                Storage.users.Add(user);

                WriteToStream(Request.OK);
                WriteToStream(user.login);
            }
            else
            {
                WriteToStream(Request.USER_ALREADY_EXIST);
                Console.WriteLine("{0} : Неудачная регистрация. Пользователь уже существует.", DateTime.Now.ToShortTimeString());
            }
            writer.Flush();
        }

        public void WriteToStream(string message)
        {
            if (writer != null)
            {
                writer.Write(message);
            }
        }

        public BinaryReader GetReaderStream()
        {
            return reader;
        }

        public void CloseClient()
        {
            reader.Close();
            writer.Close();
            tcpClient.Close();
            stream.Close();
            server.RemoveOnlineUser(this);
            if (user != null)
                user.online = false;
        }
    }
}
