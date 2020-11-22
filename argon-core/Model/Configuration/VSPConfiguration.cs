using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VSPConfiguration 
    {
        public List<VSPBinding> Bindings { get; set;}

        public VSPConfiguration() 
        {
        }
    }
}