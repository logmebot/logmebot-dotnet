using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logmebot.Net
{
    public class LogMeBotException : Exception
    {

        private int statusCode;
        public int StatusCode
        {
            set { statusCode = value; }
            get { return statusCode; }
        }

        public LogMeBotException(int statusCode, string message) : base(message)
        {
            this.StatusCode = statusCode;
        }
    }
}
