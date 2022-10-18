using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataModel.Authentication
{
    public class AuthenticateResponse
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
