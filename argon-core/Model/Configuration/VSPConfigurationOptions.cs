using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VSPConfigurationOptions : ConfigurationSection
    {
        public string Name { get; set; }
        
        public List<VSPBindingOptions> Bindings { get; set; }

        public VSPConfigurationOptions(IConfiguration configuration) : base(configuration)
        {
            Bind("vspConfiguration");
        }

    }
}