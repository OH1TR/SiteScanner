using DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScannerWebsite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
 
        private readonly ILogger<DataController> _logger;
        private ScannerModel _model;

        public DataController(ILogger<DataController> logger, ScannerModel model)
        {
            _logger = logger;
            _model = model;
        }

        [Authorize]
        [HttpGet("ScheduledWorkItems")]
        public IEnumerable<ScheduledWorkItem> ScheduledWorkItems()
        {
            return _model.GetScheduledWorkItems();
        }
    }
}
