using System.Collections;
using System.Collections.Generic;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Configuration;

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    /// Interface to be implemented by a VSP provider registry/factory
    /// </summary>
    public interface IVSPFactory
    {
        /// <summary>
        /// Returns a list of currently reigstered VSP providers
        /// </summary>
        /// <returns></returns>
        public List<VSPBinding> GetBindings();

        /// <summary>
        /// Get the provider interface for a given tag
        /// </summary>
        /// <param name="tag">The unique tag for the VSP provider</param>
        /// <returns>An in-scope implementation of the <see cref="IVSPProvider"/> interface</returns>
        /// <exception></exception>
        public IVSPProvider GetProvider(string tag);
    }
}