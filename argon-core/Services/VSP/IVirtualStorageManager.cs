using System;
using System.Collections;
using System.Collections.Generic;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Configuration;

namespace JCS.Argon.Services.VSP
{
    /// <summary>
    /// Interface to be implemented by a VSP provider registry/factory
    /// </summary>
    public interface IVirtualStorageManager
    {

        public sealed class VirtualStorageManagerException : ResponseAwareException
        {
            public VirtualStorageManagerException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(IVirtualStorageManager);
            }

            public VirtualStorageManagerException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(IVirtualStorageManager);
            }
        }
        
        /// <summary>
        /// Returns a list of currently reigstered VSP providers
        /// </summary>
        /// <returns></returns>
        public List<VirtualStorageBinding> GetBindings();

        /// <summary>
        /// Get the provider interface for a given tag
        /// </summary>
        /// <param name="tag">The unique tag for the VSP provider</param>
        /// <returns>An in-scope implementation of the <see cref="IVirtualStorageProvider"/> interface</returns>
        /// <exception></exception>
        public IVirtualStorageProvider GetProvider(string tag);
    }
}