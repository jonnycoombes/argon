using Microsoft.Extensions.Configuration;

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
        /// <param name="source"></param>
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
    }
}