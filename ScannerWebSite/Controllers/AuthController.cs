using DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScannerWebSite.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScannerWebSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ScannerModel _scannerModel;

        public AuthController(ILogger<AuthController> logger, ScannerModel scannerModel)
        {
            _logger = logger;
            _scannerModel = scannerModel;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            return Ok();
        }
    }
}
