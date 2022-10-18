using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Archive
{
    public class HTTPTransaction
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Guid ArchiveId { get; set; }
        public Guid ScanId { get; set; }

        public int Time { get; set; }

        public Request Request { get; set; }

        public Response Response { get; set; }

        public DateTime StartedDateTime { get; set; }

        public string ServerIpAddress { get; set; }
    }

    public class Request
    {
        public string Method { get; set; }

        public string Url { get; set; }

    }

    public class Response
    {

        public Header[] Headers { get; set; }

        public int Status { get; set; }

        public int BodySize { get; set; }

        public string StatusText { get; set; }

        public string RedirectUrl { get; set; }
    }

    public class Header
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
