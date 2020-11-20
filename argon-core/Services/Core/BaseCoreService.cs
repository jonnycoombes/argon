using JCS.Argon.Contexts;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Services.Core
{
    public abstract class BaseCoreService
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected ILogger _log;

        /// <summary>
        /// An underlying DB context
        /// </summary>
        protected SqlDbContext _dbContext;
        
        public BaseCoreService(ILogger log, SqlDbContext dbContext)
        {
            _log = log;
            _dbContext = dbContext;
        }
    }
}