using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    /// <summary>
    /// An enumeration of the currently implemented constraint types
    /// </summary>
    public enum ConstraintType
    {
        Mandatory,
        Mapping,
        AllowableType,
        AllowableTypeAndValues
    }

    /// <summary>
    /// Enumeration of the different type coercions available within constraints
    /// </summary>
    public enum ConstraintValidTypes
    {
        String,
        Number,
        DateTime,
        Boolean
    }
    
    /// <summary>
    /// Defines an individual constraint that can be imposed upon items within a collection.
    /// The interpretation of a given constraint may be dependent on the underlying VSP
    /// provider for the collection. 
    /// </summary>
    [Table("constraint", Schema = "core")]
    public class Constraint
    {
        /// <summary>
        /// The concurrency token for the object
        /// </summary>
        [JsonIgnore]
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    
        /// <summary>
        /// The unique identifier for the constraint
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid? Id { get; set; }

        /// <summary>
        /// The required name for the constraint.  The name of a given constraint
        /// does not necessarily need to be unique
        /// </summary>
        [Required] 
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// The type of the constraint, must be a value taken from <see cref="ConstraintType"/>
        /// </summary>
        [Required]
        public ConstraintType ConstraintType { get; set; }
        
        /// <summary>
        /// The source property for the constraint
        /// </summary>
        [Required]
        public string SourceProperty { get; set; } = null!;
        
        /// <summary>
        /// The target property for the constaint.  Only applicable if the constaint is of "mapping" type
        /// </summary>
        public string? TargetProperty { get; set; }
        
        /// <summary>
        /// Optional field which specifies the valid type for values enforced by this constraint
        /// </summary>
        public ConstraintValidTypes? ValueType { get; set; }
        
        /// <summary>
        /// An optional list of allowable values for the property guarded by the constraint
        /// </summary>
        public string[]? AllowableValues { get; set; }
    }
}