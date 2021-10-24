using DataModel;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ScannerBot.Services
{
    class Filter
    {
        List<FilteredIP> FilteredIPs;
        List<SlowIP> SlowIPs;
        ScannerModel _model;
        public Filter(ScannerModel model)
        {
            _model = model;
            FilteredIPs =model.GetAllItems<FilteredIP>();
            SlowIPs = model.GetAllItems<SlowIP>();
        }

        public bool IsFiltered(IPAddress ip,out string reason)
        {
            reason = "";
            foreach (var i in FilteredIPs)
            {
                if (IPAddress.Parse(i.IPAddress) == ip)
                {
                    reason = i.Reason;
                    return true;
                }
            }
            return false;
        }

        public bool IsSlowIP(IPAddress ip)
        {
            foreach (var i in SlowIPs)
            {
                if (IPAddress.Parse(i.IPAddress) == ip)
                    return true;
            }
            return false;
        }

        public void AddSlowIP(IPAddress ip)
        {
            SlowIPs.Add(_model.AddItem(new SlowIP() { Id = Guid.NewGuid(), Created = DateTime.UtcNow, IPAddress = ip.ToString() }));
        }
    }
}
