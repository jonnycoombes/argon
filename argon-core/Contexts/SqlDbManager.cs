#region

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#endregion

namespace JCS.Argon.Contexts
{
    /// <summary>
    ///     This class can be used to perform any startup/initialisation logic against the database
    ///     at application startup
    /// </summary>
    public class SqlDbManager
    {
        public static void DoStartupTasks(IWebHost webHost)
        {
            using var scope = webHost.Services.CreateScope();
            {
                var log = scope.ServiceProvider.GetRequiredService<ILogger<SqlDbManager>>();
            }
        }
    }
}