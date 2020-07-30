using System;
using System.Collections.Generic;
using System.Text;

namespace webServer
{
    class Request
    {
        public const string REGISTR = "0000";
        public const string LOGIN = "0001";
        public const string OK = "0010";
        public const string USER_ALREADY_EXIST = "0011";
        public const string USER_DOES_NOT_EXIST = "0100";
        public const string INVALID_PASSWORD = "0101";
    }
}
