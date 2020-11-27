using System;
using System.Threading.Tasks;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// The main interface for classes that can managed property sets for various objects within
    /// Argon.  The management of properties for documents, collections etc...is split into a separate
    /// structure so that it may be replaced with implementations backed by say Mongo, Elastic, CouchDB
    /// etc...The default implementation will be based on a SQL Server underlying store, however PostgreSQL
    /// would make a pretty good alternative so that properties can be persisted as natively-supported
    /// JSON column values.
    /// </summary>
    public interface IPropertyGroupManager
    {
        /// <summary>
        /// Thrown in the event of a failure within the constraint group manager
        /// </summary>
        public sealed class PropertyGroupManagerException: ResponseAwareException
        {
            public PropertyGroupManagerException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(IPropertyGroupManager);
            }

            public PropertyGroupManagerException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(IPropertyGroupManager); 
            }
        }

        /// <summary>
        /// Creates a new (empty) property group
        /// </summary>
        /// <returns></returns>
        public Task<PropertyGroup> CreatePropertyGroupAsync();

    }
}