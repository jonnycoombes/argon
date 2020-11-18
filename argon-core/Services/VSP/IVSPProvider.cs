using JCS.Argon.Model.Configuration;

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    /// The core interface that must be implemented by each distinct VSP provider
    /// </summary>
    public interface IVSPProvider
    {
        /// <summary>
        /// Return the <see cref="VSPBinding"/> for this provider
        /// </summary>
        /// <returns></returns>
        public VSPBinding GetConfiguration();
    }
}