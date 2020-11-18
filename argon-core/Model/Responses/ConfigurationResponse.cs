using System.Collections.Generic;
using JCS.Argon.Model.Configuration;

namespace JCS.Argon.Model.Responses
{
    public class ConfigurationResponse
    {
        public List<VSPBinding> Bindings { get; set; } = null!;
    }
}