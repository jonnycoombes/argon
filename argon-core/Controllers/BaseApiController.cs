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
    }
}