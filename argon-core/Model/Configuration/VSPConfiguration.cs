using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VSPConfiguration : ConfigurationSection
    {
        public List<VSPBinding> Bindings { get; init; }

        public VSPConfiguration(IConfiguration configuration) : base(configuration)
        {
            Bind("vsp");
        }

        public override void DumpToLog(ILogger log)
        {
            foreach (var binding in Bindings)
            {
                log.LogDebug($"Found a VSP provider: {binding}");
            }
        }
    }
}