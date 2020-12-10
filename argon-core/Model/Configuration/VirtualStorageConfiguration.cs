using System.Collections.Generic;

#pragma warning disable 8618

namespace JCS.Argon.Model.Configuration
{
    public class VirtualStorageConfiguration 
    {
        public VirtualStorageConfiguration() 
        {
        }

        public List<VirtualStorageBinding> Bindings { get; set;}
    }
}