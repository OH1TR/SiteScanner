using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Archive
{
    public class Address
    {
        public string Ip { get; set; }
        public string Cidr { get; set; }
        public int Asn { get; set; }
        public string Desc { get; set; }
    }

    public class SubDomain
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        public List<Address> Addresses { get; set; }
        public string Tag { get; set; }
        public List<string> Sources { get; set; }
    }
}
