using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    [Table("item", Schema = "argon")]
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
        [Column(TypeName = "varchar(512)")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The created date for the item
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last created time for the item
        /// </summary>
        [Required]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// This items parent <see cref="Collection"/>
        /// </summary>
        [Required]
        public Collection Collection { get; set; } = null!;

        /// <summary>
        /// The <see cref="System.Version"/> versions for this item
        /// </summary>
        public List<ItemVersion> Versions { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        public PropertyGroup PropertyGroup { get; set; } = null!;
    }
}