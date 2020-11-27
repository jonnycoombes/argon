using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace JCS.Argon.Model.Schema
{
    public enum PropertyType
    {
        String,
        Number,
        DateTime,
        Boolean
    }

    [Table("property", Schema = "core")]
    public class Property
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
        /// The name of the property
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The type of the property
        /// </summary>
        [Required]
        public PropertyType Type { get; set; }
        
        /// <summary>
        /// Optional string value for the property
        /// </summary>
        public string? StringValue { get; set; }
        
        /// <summary>
        /// Optional numeric value for the property
        /// </summary>
        public double? NumberValue { get; set; }
        
        /// <summary>
        /// Optional date-time value for the property
        /// </summary>
        public DateTime? DateTimeValue { get; set; }
        
        /// <summary>
        /// Optional boolean value for the property
        /// </summary>
        public bool? BooleanValue { get; set; }
        
        /// <summary>
        /// The identifier for the parent property group
        /// </summary>
        [Required]
        public Guid PropertyGroupId { get; set; }
        
        /// <summary>
        /// The parent <see cref="PropertyGroup"/>
        /// </summary>
        public PropertyGroup PropertyGroup { get; set; } = null!;
    }
}