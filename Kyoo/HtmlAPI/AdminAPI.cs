﻿using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Kyoo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
	    [HttpGet("scan")]
        public IActionResult ScanLibrary([FromServices] ICrawler crawler)
        {
	        // The crawler is destroyed before the completion of this task.
	        // TODO implement an hosted service that can queue tasks from the controller.
            crawler.StartAsync(new CancellationToken());
            return Ok("Scanning");
        }
    }
}