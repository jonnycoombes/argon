using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Controllers
{
    /// <summary>
    /// Abstract base class for all API controllers
    /// </summary>
    public class BaseApiController : ControllerBase
    {
        protected ILogger _log;

        protected BaseApiController(ILogger log)
        {
            _log = log;
        }

        /// <summary>
        /// Helper function which attempts to decode any supplied properties within an inbound request
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, object>? ExtractPropertiesFromRequest()
        {
            Dictionary<string, object>? properties = null;
            if (Request.Form.ContainsKey("Properties"))
            {
                properties = JsonSerializer.Deserialize<Dictionary<string, object>>(Request.Form["Properties"]);
            }

            return properties;
        }
    }
}