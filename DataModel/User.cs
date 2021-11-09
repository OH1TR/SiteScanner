using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class User
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
