using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Exceptions;
using JCS.Argon.Model.Schema;

namespace JCS.Argon.Services.Core
{
    /// <summary>
    /// The main interface for classes that can managed constraint sets for collections within
    /// Argon.  The management of constraints for documents, collections etc...is split into a separate
    /// structure so that it may be replaced with implementations backed by say Mongo, Elastic, CouchDB
    /// etc...The default implementation will be based on a SQL Server underlying store, however PostgreSQL
    /// would make a pretty good alternative so that properties can be persisted as natively-supported
    /// JSON column values.
    /// </summary>
    public interface IConstraintGroupManager
    {
        
        /// <summary>
        /// Thrown in the event of a failure within the constraint group manager
        /// </summary>
        public sealed class ConstraintGroupManagerException: ResponseAwareException
        {
            public ConstraintGroupManagerException(int? statusHint, string? message) : base(statusHint, message)
            {
                Source = nameof(ICollectionManager);
            }

            public ConstraintGroupManagerException(int? statusHint, string? message, Exception? inner) : base(statusHint, message, inner)
            {
                Source = nameof(ICollectionManager); 
            }
        }
        
        /// <summary>
        /// Create an empty <see cref="ConstraintGroup"/> and return it
        /// </summary>
        /// <returns></returns>
        public Task<ConstraintGroup> CreateConstraintGroupAsync();

        /// <summary>
        /// Create a new <see cref="ConstraintGroup"/> based on a supplied list of <see cref="Constraint"/>
        /// definitions (or creation commands)
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Task<ConstraintGroup> CreateConstraintGroupAsync(List<CreateConstraintCommand> constraints);

        /// <summary>
        /// Creates a new constraint and performs any necessary validation of the create command
        /// </summary>
        /// <param name="cmd">A valid <see cref="CreateConstraintCommand"/></param>
        /// <returns>A new <see cref="Constraint"/></returns>
        public Task<Constraint> CreateConstraintAsync(CreateConstraintCommand cmd);
    }
}