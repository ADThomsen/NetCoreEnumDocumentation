using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreEnumDocumentation.Model;

namespace NetCoreEnumDocumentation.Controllers
{
    [ApiController]
    [Route("api/demo")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    internal class DemoController : ControllerBase
    {
        // dummy just to show that dependency injection works in an internal class
        private readonly ILogger<DemoController> _logger;

        public DemoController(ILogger<DemoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ApiVersion("1.0")]
        public IActionResult Get()
        {
            return Ok("v1");
        }

        [HttpGet]
        [ApiVersion("2.0")]
        public IActionResult GetV2([FromQuery]string input)
        {
            return Ok(input);
        }

        [HttpGet("next")]
        public Basket SomeOtherMethod()
        {
            return new Basket();
        }
    }
}