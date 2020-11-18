using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace JCS.Argon.Controllers
{
    /// <summary>
    /// Common abstract base class for Api controllers throughout the core. Common logging, fault handling logic etc can be placed within this class
    /// </summary>
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// The logger 
        /// </summary>
        protected ILogger _log;

        protected ApiControllerBase()
        {
            
        }
        
    }
}