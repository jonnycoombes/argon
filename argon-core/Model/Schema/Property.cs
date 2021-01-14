using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    public enum PropertyType
    {
        String,
        Number,
        DateTime,
        Boolean
    }

    [Table("property", Schema = "argon")]
    public class Property
    {
        public Property()
        {
            
        }

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
        [JsonIgnore]
        public Guid PropertyGroupId { get; set; }

        /// <summary>
        /// The parent <see cref="PropertyGroup"/>
        /// </summary>
        [JsonIgnore]
        public PropertyGroup PropertyGroup { get; set; } = null!;

        /// <summary>
        /// Convenience method for clearing the current value
        /// </summary>
        public void ClearValue()
        {
            switch (Type)
            {
                case PropertyType.Boolean:
                    BooleanValue = null;
                    break;
                case PropertyType.Number:
                    NumberValue = null;
                    break;
                case PropertyType.String:
                    StringValue = null;
                    break;
                case PropertyType.DateTime:
                    DateTimeValue = null;
                    break;
            }
        }
    }
}