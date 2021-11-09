using DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScannerBot.Services
{
    class Scheduler
    {
        private ScannerModel _scannerModel;

        public Scheduler(ScannerModel scannerModel)
        {
            _scannerModel = scannerModel;
        }

        public void Process()
        {
            DateTime now = DateTime.UtcNow;

            var items = _scannerModel.GetAllItems<ScheduledWorkItem>();
            foreach (var item in items)
            {
                if (item.LastScheduledTime < now)
                {
                    while (item.LastScheduledTime < now)
                        item.LastScheduledTime = item.LastScheduledTime.AddSeconds(item.Interval);
                    _scannerModel.UpdateItem(item);
                    item.Work.Created = now;
                    _scannerModel.PushWorkItem(item.Work);
                }
            }
        }

    }
}
