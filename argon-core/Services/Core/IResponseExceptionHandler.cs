using System.Threading.Tasks;
using JCS.Argon.Model.Responses;
using Microsoft.AspNetCore.Http;

namespace JCS.Argon.Services.Core
{
    public interface IResponseExceptionHandler
    {
        /// <summary>
        /// Extract and then handle an exception, given a valid <see cref="HttpContext"/>
        /// instance
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/></param>
        public ExceptionResponse GenerateExceptionResponseFromContext(HttpContext context);
    }
}