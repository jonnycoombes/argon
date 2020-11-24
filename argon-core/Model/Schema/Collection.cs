using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    [Table("collection", Schema = "core")]
    public class Collection
    {
        /// <summary>
        /// The primary concurrency token for this entity
        /// </summary>
        [JsonIgnore]
        [Timestamp]
        public byte[]? Timestamp { get; set; }
        
        /// <summary>
        /// The unique identifier for the collection
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid? Id { get; set; }
        
        /// <summary>
        /// The name of the collection
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The description for the collection (optional)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The length of the collection in terms of number of collection items
        /// </summary>
        public long Length { get; set; } = 0;

        /// <summary>
        /// The aggregate size of the collection in bytes
        /// </summary>
        public long Size { get; set; } = 0;
        
        /// <summary>
        /// The items associated with this collection
        /// </summary>
        [JsonIgnore]
        public List<Item> Items { get; set; } = null!;

        /// <summary>
        /// The optional unique id associated with the propeties for the collection
        /// </summary>
        public Guid? PropertyGroupId { get; set; } = null!;

        /// <summary>
        /// Default constructor - required for initialiser syntax
        /// </summary>
        public Collection()
        {
            
        }

    }
}