using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Logmebot.Net.Models
{
    /// <summary>
    /// Model for /AccessToken api result
    /// </summary>
    public class AccessTokenResult
    {
        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }
    }
}