using System.ComponentModel.DataAnnotations;

namespace JCS.Argon.Model.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateCollectionCommand
    {
        public CreateCollectionCommand(string name, string providerType, string? description)
        {
            Name = name;
            ProviderType = providerType;
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
        /// The provider to be used to create the collection.  Must equate to a provider type
        /// listed within the current set of registered VSP bindings.
        /// </summary>
        [Required]
        public string ProviderType { get; set; }
        
        /// <summary>
        /// Optional description for the collection.
        /// </summary>
        public string? Description { get; set; }

    }
}