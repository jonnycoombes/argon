using System.Collections;
using System.Collections.Generic;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : BaseApiController
    {
        protected readonly IVSPFactory IvspFactory;
        
        public ConfigurationController(ILogger<ConfigurationController> log, IVSPFactory registry) : base(log)
        {
            Log.LogInformation("Instantiating new instance");
            IvspFactory = registry;
        }

        [HttpGet]
        public IEnumerable<VSPBindingOptions> Get()
        {
            return IvspFactory.GetConfigurations();
        }
    }
}