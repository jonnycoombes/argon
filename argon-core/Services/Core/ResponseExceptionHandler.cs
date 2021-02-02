#region

using JCS.Argon.Model.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

#endregion

namespace JCS.Argon.Services.Core
{
    public class ResponseExceptionHandler : IResponseExceptionHandler
    {
        protected ILogger _log;

        public ResponseExceptionHandler(ILogger<ResponseExceptionHandler> log)
        {
            _log = log;
        }

        public ExceptionResponse GenerateExceptionResponseFromContext(HttpContext context)
        {
            _log.LogWarning("Handling a new exception");
            _log.LogWarning("Extracting exception from the supplied context");
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature.Error != null)
            {
                var ex = exceptionHandlerPathFeature.Error;
                _log.LogWarning($"Found an exception of type {ex.GetType()}");
                return ex switch
                {
                    ICollectionManager.CollectionManagerException e => new ExceptionResponse
                    {
                        HttpResponseCode = e.ResponseCodeHint ?? StatusCodes.Status500InternalServerError,
                        Message = e.Message,
                        Source = e.Source
                    },
                    IConstraintGroupManager.ConstraintGroupManagerException e => new ExceptionResponse
                    {
                        HttpResponseCode = e.ResponseCodeHint ?? StatusCodes.Status500InternalServerError,
                        Message = e.Message,
                        Source = e.Source
                    },
                    IPropertyGroupManager.PropertyGroupManagerException e => new ExceptionResponse
                    {
                        HttpResponseCode = e.ResponseCodeHint ?? StatusCodes.Status500InternalServerError,
                        Message = e.Message,
                        Source = e.Source
                    },
                    _ => new ExceptionResponse
                    {
                        HttpResponseCode = StatusCodes.Status500InternalServerError, Message = ex.Message, Source = ex.Source
                    }
                };
            }

            _log.LogWarning("Didn't locate an exception in the current context");
            return new ExceptionResponse();
        }
    }
}