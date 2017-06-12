using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Logmebot.Net.Models
{
    /// <summary>
    /// model for /Me api result
    /// give info about current logged user such as email, user_id etc..
    /// </summary>
    public class MeResult
    {
        public string UserId { get; set; }

        public string Email { get; set; }

        public string Nickname { get; set; }

        public bool Expired { get; set; }

        public int ExpiresIn { get; set; }
    }
}