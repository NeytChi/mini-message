using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace mini_message.Controllers
{
    [ApiController]
    [Route("v1.0/[controller]/[action]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }
    }
}
