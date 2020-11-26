using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        [JsonIgnore]
        public Guid? Id { get; set; }
        
        /// <summary>
        /// The properties in the bag
        /// </summary>
        public List<Property>? Properties { get; set; }

        /// <summary>
        /// Helper function for searching for a given property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool HasProperty(string propertyName)
        {
            return Properties != null && Properties.Any(p => p.Name.Equals(propertyName));
        }

        public Property? GetPropertyByName(string propertyName)
        {
            if (HasProperty(propertyName))
            {
                return Properties?.First(p => p.Name.Equals(propertyName));
            }
            else
            {
                return null;
            }
        }
    }
}