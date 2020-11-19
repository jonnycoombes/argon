using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class CollectionsController : BaseApiController
    {
        /// <summary>
        /// An instance of <see cref="IVSPFactory"/> used in order to obtain instances of <see cref="IVSPProvider"/> so that
        /// operations may be performed against a given collection
        /// </summary>
        protected readonly IVSPFactory _vspFactory;
        
        /// <summary>
        /// Default constructor.  Parameters are DI'd at runtime
        /// </summary>
        /// <param name="log">A logger for logging</param>
        /// <param name="vspFactory">An instance of <see cref="IVSPFactory"/></param>
        protected CollectionsController(ILogger<CollectionsController> log, IVSPFactory vspFactory) : base(log)
        {
            Log.LogInformation("Creating new instance");
            _vspFactory = vspFactory;
        }
        
    }
}