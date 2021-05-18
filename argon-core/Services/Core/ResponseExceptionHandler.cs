#region

using JCS.Argon.Model.Responses;
using JCS.Argon.Services.VSP;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

namespace JCS.Argon.Services.Core
{
    public class ResponseExceptionHandler : IResponseExceptionHandler
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<ResponseExceptionHandler>();

        public ExceptionResponse GenerateExceptionResponseFromContext(HttpContext context)
        {
            LogWarning(_log, "Handling a new exception");
            LogWarning(_log, "Extracting exception from the supplied context");
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature.Error != null)
            {
                var ex = exceptionHandlerPathFeature.Error;
                LogWarning(_log, $"Found an exception of type {ex.GetType()}");
                return ex switch
                {
                    ICollectionManager.CollectionManagerException e => new ExceptionResponse
                    {
                        HttpResponseCode = e.ResponseCodeHint ?? StatusCodes.Status500InternalServerError,
                        Message = e.Message,
                        Source = e.Source
                    },
                    IItemManager.ItemManagerException e => new ExceptionResponse
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
                    IVirtualStorageManager.VirtualStorageManagerException e => new ExceptionResponse
                    {
                        HttpResponseCode = e.ResponseCodeHint ?? StatusCodes.Status500InternalServerError,
                        Message = e.Message,
                        Source = e.Source
                    },
                    ArchiveManagerException e => new ExceptionResponse
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

            LogWarning(_log, "Didn't locate an exception in the current context");
            return new ExceptionResponse();
        }
    }
}