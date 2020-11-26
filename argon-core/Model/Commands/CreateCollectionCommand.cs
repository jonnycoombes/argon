using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JCS.Argon.Model.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateCollectionCommand
    {
        public CreateCollectionCommand(string name, string providerTag, string? description)
        {
            Name = name;
            ProviderTag = providerTag;
            Description = description;
        }

        /// <summary>
        /// Name for the new collection.  Must not already be registered.  Although
        /// collections are unique based on system-allocated GUIDS, it is good practice
        /// to not create collections with identical names.  This restriction may be removed in
        /// future releases.
        /// </summary>
        [Required]
        public string Name { get; set; }
        
        /// <summary>
        /// The provider to be used to create the collection.  Must equate to a provider tag
        /// listed within the current set of registered VSP bindings.
        /// </summary>
        [Required]
        public string ProviderTag { get; set; }
        
        /// <summary>
        /// Optional description for the collection.
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// An optional list of <see cref="JCS.Argon.Model.Schema.Constraint"/> elements
        /// for the collection
        /// </summary>
        public List<CreateConstraintCommand>? Constraints { get; set; }

    }
}