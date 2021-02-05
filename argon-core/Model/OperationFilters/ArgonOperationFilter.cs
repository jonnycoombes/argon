using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JCS.Argon.Model.OperationFilters
{
    /// <summary>
    ///     Custom operation filter which ensures that item upload operations have the correct signature generated within the
    ///     default Swagger/Swashbuckle UI
    /// </summary>
    public class ArgonOperationFilter : IOperationFilter
    {
        /// <summary>
        ///     The Apply method - looks for operations hanging off the ItemController and related to upload and then
        ///     adjusts the parameters
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
        }
    }
}