using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : BaseApiController
    {

        public ConfigurationController(ILogger<ConfigurationController> log) : base(log)
        {
            Log.LogInformation("Instantiating new instance");
        }

        [HttpGet]
        public string Get()
        {
            return "Hello world";
        }
    }
}