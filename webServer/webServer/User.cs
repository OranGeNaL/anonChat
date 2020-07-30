using System;
using System.Collections.Generic;
using System.Text;

namespace webServer
{
    public class User
    {
        public string password { get; private set; }
        public string login { get; private set; }
        public bool online = false;

        public User(string _login, string _password)
        {
            password = _password;
            login = _login;
        }

        public override string ToString()
        {
            return String.Format("Логин: {0}\nПароль: {1}", login, password);
        }
    }
}
