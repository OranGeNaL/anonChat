using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace consoleClient
{
    public static class IP
    {
        public const string local = "127.0.0.1";
        public const string external = "46.37.131.29";
    }

    class Program
    {
        const int PORT = 5505;
        const string ADRESS = IP.external;

        static void Main(string[] args)
        {
            while(true)
            {
                Console.WriteLine("1. Регистрация");
                Console.WriteLine("2. Авторизация");
                int answer = int.Parse(Console.ReadLine());
                switch (answer)
                {
                    case 1:
                        Registration();
                        break;
                    case 2:
                        Authorization();
                        break;
                    default:
                        Console.WriteLine("Такого варианта нет!");
                        break;
                }
            }
        }

        private static void Chat(BinaryReader reader, BinaryWriter writer)
        {
            Console.Clear();
            Thread messageGetThread = new Thread(new ParameterizedThreadStart(MessageGetter));
            messageGetThread.Start(reader);
            try
            {
                Console.WriteLine("Введите сообщение и нажмите Enter. Для выхода введите <!exit>.");
                while(true)
                {
                    string message = Console.ReadLine();
                    if(message != "!exit")
                    {
                        writer.Write(message);
                    }
                    else
                    {
                        messageGetThread.Abort();
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void MessageGetter(object readerObj)
        {
            try
            {
                BinaryReader reader = (BinaryReader)readerObj;
                while (true)
                {
                    string message = reader.ReadString();
                    Console.WriteLine(message);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        private static void Authorization()
        {
            TcpClient tcpClient = null;

            Console.Write("Введите логин: ");
            string login = Console.ReadLine();

            try
            {
                tcpClient = new TcpClient(ADRESS, PORT);
                NetworkStream stream = tcpClient.GetStream();

                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(Request.LOGIN);
                if(reader.ReadString() == Request.OK)
                {
                    writer.Write(login);
                    if(reader.ReadString() != Request.USER_DOES_NOT_EXIST)
                    {
                        Console.WriteLine("Пользователь найден.");
                        Console.Write("Введите пароль: ");
                        string password = PasswrdToHash(Console.ReadLine());
                        writer.Write(password);
                        if(reader.ReadString() != Request.INVALID_PASSWORD)
                        {
                            Console.WriteLine("Успешная авторизация под ником {0}.", login);
                            Chat(reader, writer);
                            reader.Close();
                            writer.Close();
                            tcpClient.Close();
                            stream.Close();
                        }
                        else
                        {
                            Console.WriteLine("Ошибка авторизации! Неверный пароль.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка авторизации! Пользователь не существует.");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Registration()
        {
            TcpClient tcpClient = null;

            Console.WriteLine("Регистрация");
            Console.Write("Введите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            try
            {
                tcpClient = new TcpClient(ADRESS, PORT);
                NetworkStream stream = tcpClient.GetStream();

                BinaryWriter writer = new BinaryWriter(stream);
                BinaryReader reader = new BinaryReader(stream);

                writer.Write(Request.REGISTR);
                if (reader.ReadString() == Request.OK)
                {
                    Console.WriteLine("Соединение стабильно, передача данных...");
                    writer.Write(login);
                    writer.Write(PasswrdToHash(password));

                    string regRes = reader.ReadString();
                    if (regRes != Request.USER_ALREADY_EXIST)
                    {
                        string loginRes = reader.ReadString();
                        Console.WriteLine("Произошла регистрация под логином {0}", loginRes);
                    }
                    else
                    {
                        Console.WriteLine("Неудачная попытка регистрации. Пользователь с таким логином уже существует.");
                    }
                    reader.Close();
                    writer.Close();
                    tcpClient.Close();
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static string PasswrdToHash(string password)
        {
            var md5Hash = MD5.Create();
            var passwdHash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(passwdHash).Replace("-", String.Empty);
        }
    }
}
