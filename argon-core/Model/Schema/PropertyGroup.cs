using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    [Table(name:"propertyGroup", Schema = "core")]
    public class PropertyGroup
    {
        /// <summary>
        /// The primary concurrency token for this entity
        /// </summary>
        [JsonIgnore]
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    
        /// <summary>
        /// The unique identifier for the version
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid? Id { get; set; }
        
        /// <summary>
        /// The properties in the bag
        /// </summary>
        public List<Property> Properties { get; set; } = null!;
    }
}