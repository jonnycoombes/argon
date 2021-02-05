#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JCS.Argon.Model.Schema;

#endregion

namespace JCS.Argon.Model.Commands
{
    /// <summary>
    ///     Command for the creation of new <see cref="Collection" /> instances
    /// </summary>
    public class CreateCollectionCommand
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        public CreateCollectionCommand()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="name">The name of the new collection</param>
        /// <param name="providerTag">The tag for the provider to use in order to create the collection</param>
        /// <param name="description">An optional description for the collection</param>
        public CreateCollectionCommand(string name, string providerTag, string? description)
        {
            Name = name;
            ProviderTag = providerTag;
            Description = description;
        }

        /// <summary>
        ///     Name for the new collection.  Must not already be registered.  Although
        ///     collections are unique based on system-allocated GUIDS, it is good practice
        ///     to not create collections with identical names.  This restriction may be removed in
        ///     future releases.
        /// </summary>
        [Required(ErrorMessage = "You must specify a name for the collection")]
        public string Name { get; set; }

        /// <summary>
        ///     The provider to be used to create the collection.  Must equate to a provider tag
        ///     listed within the current set of registered VSP bindings.
        /// </summary>
        [Required(ErrorMessage = "You must specify a provider tag, so that correct collection storage provider may be selected")]
        public string ProviderTag { get; set; }

        /// <summary>
        ///     Optional description for the collection.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     An optional list of <see cref="JCS.Argon.Model.Schema.Constraint" /> elements
        ///     for the collection
        /// </summary>
        public List<CreateOrUpdateConstraintCommand>? Constraints { get; set; }
    }
}