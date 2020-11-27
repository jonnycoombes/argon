using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VirtualStorageConfiguration 
    {
        public List<VirtualStorageBinding> Bindings { get; set;}

        public VirtualStorageConfiguration() 
        {
        }
    }
}