using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Services.VSP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    public class CollectionManager : ICollectionManager
    {

        /// <summary>
        /// The logger
        /// </summary>
        protected ILogger _log;

        /// <summary>
        /// The currently configured <see cref="IVSPFactory"/> instance
        /// </summary>
        protected IVSPFactory _vspFactory;

        /// <summary>
        /// The currently configured <see cref="SqlDbContext"/> instance
        /// </summary>
        protected SqlDbContext _dbContext;
        
        /// <summary>
        /// Default constructor, parameters are DI'd by the IoC layer
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        /// <param name="vspFactory"></param>
        public CollectionManager(ILogger<CollectionManager> log, SqlDbContext dbContext, IVSPFactory vspFactory)
        {
            _log = log;
            _dbContext = dbContext;
            _vspFactory = vspFactory;
        }
        
        public async Task<int> CollectionCountAsync()
        {
            return await _dbContext.Collections.CountAsync();
        }
    }
}