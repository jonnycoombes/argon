using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JCS.Argon.Model.Configuration
{
    public class ConfigurationSection
    {
        /// <summary>
        /// Yep - it's the configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        protected ConfigurationSection(IConfiguration configuration)
        {
            _configuration= configuration;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        protected void Bind(string source)
        {
            _configuration.Bind(source, this);
        }

        public virtual void DumpToLog(ILogger log)
        {
            log.LogDebug($"{this}");
        }
    }
}