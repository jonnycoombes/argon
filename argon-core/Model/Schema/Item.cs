using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    [Table("item", Schema = "core")]
    public class Item
    {
        /// <summary>
        /// The primary concurrency token for this entity
        /// </summary>
        [JsonIgnore]
        [Timestamp]
        public byte[]? Timestamp { get; set; }
        
        /// <summary>
        /// The unique identifier for the item
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid? Id { get; set; }
        
        /// <summary>
        /// The name of the item
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// The created date for the item
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// The last created time for the item
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// The parent collection identifier
        /// </summary>
        public Guid CollectionId { get; set;}
        
        /// <summary>
        /// This items parent <see cref="Collection"/>
        /// </summary>
        public Collection Collection { get; set; } = null!;

        /// <summary>
        /// The <see cref="System.Version"/> versions for this item
        /// </summary>
        public List<Version> Versions { get; set; } = null!;

        /// <summary>
        /// The optional property group identifier for this item
        /// </summary>
        public Guid? PropertyGroupId { get; set; } = null!;
        
        /// <summary>
        /// 
        /// </summary>
        public PropertyGroup? Properties { get; set; } = null!; 

    }
}