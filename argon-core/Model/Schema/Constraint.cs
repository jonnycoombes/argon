using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    
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
    }
}