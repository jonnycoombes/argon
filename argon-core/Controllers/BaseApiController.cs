#region

using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Controllers
{
    /// <summary>
    ///     Abstract base class for all API controllers
    /// </summary>
    public class BaseApiController : ControllerBase
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<BaseApiController>();

        /// <summary>
        ///     Default constructor
        /// </summary>
        protected BaseApiController()
        {
            LogMethodCall(_log);
        }

        /// <summary>
        ///     Helper function which attempts to decode any supplied properties within an inbound request
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, object>? ExtractPropertiesFromRequest()
        {
            LogMethodCall(_log);
            Dictionary<string, object>? properties = null;
            if (Request.Form.ContainsKey("Properties"))
                properties = JsonSerializer.Deserialize<Dictionary<string, object>>(Request.Form["Properties"]);

            return properties;
        }
    }
}