using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    [Table("constraintGroup", Schema = "core")]
    public class ConstraintGroup
    {
        /// <summary>
        /// The primary concurrency token for this entity
        /// </summary>
        [JsonIgnore]
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    
        /// <summary>
        /// The unique identifier for the constraint group
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [JsonIgnore]
        public Guid? Id { get; set; }

        /// <summary>
        /// The constraints for this group
        /// </summary>
        public List<Constraint> Constraints { get; set; }

        /// <summary>
        /// Utility function for locating a given constraint
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <returns></returns>
        public bool HasPropertyConstraint(string sourceProperty)
        {
            return Constraints.Any(c => c.SourceProperty.Equals(sourceProperty));
        }

        /// <summary>
        /// Utility function for quickly accessing a specific constraint
        /// </summary>
        /// <param name="sourceProperty">The name of the property that the constraint applies to</param>
        /// <returns></returns>
        public Constraint? GetConstraintBySourceProperty(string sourceProperty)
        {
            if (HasPropertyConstraint(sourceProperty))
            {
                return Constraints?.First(c => c.SourceProperty.Equals(sourceProperty));
            }
            else
            {
                return null;
            }
        }
    }
}