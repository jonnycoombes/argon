#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

#endregion

namespace JCS.Argon.Model.Schema
{
    /// <summary>
    ///     Aggregate entity for <see cref="Property" /> entities
    /// </summary>
    [Table("propertyGroup", Schema = "argon")]
    public class PropertyGroup
    {
        /// <summary>
        ///     The primary concurrency token for this entity
        /// </summary>
        [JsonIgnore]
        [Timestamp]
        public byte[]? Timestamp { get; set; }

        /// <summary>
        ///     The unique identifier for the version
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [JsonIgnore]
        public Guid? Id { get; set; }

        /// <summary>
        ///     The properties in the bag
        /// </summary>
        public List<Property>? Properties { get; set; }

        /// <summary>
        ///     Adds or replaces a property
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="type">The required property type</param>
        /// <param name="value">The value for the property</param>
        public void AddOrReplaceProperty(string name, PropertyType type, object value)
        {
            if (HasProperty(name))
            {
                var prop = GetPropertyByName(name);
                if (prop == null) return;
                if (prop.Type != type)
                {
                    prop.ClearValue();
                    prop.Type = type;
                }

                switch (type)
                {
                    case PropertyType.Boolean:
                        prop.BooleanValue = (bool) value;
                        break;
                    case PropertyType.Number:
                        prop.NumberValue = Convert.ToDouble(value);
                        break;
                    case PropertyType.DateTime:
                        prop.DateTimeValue = (DateTime) value;
                        break;
                    case PropertyType.String:
                        prop.StringValue = value.ToString();
                        break;
                    default:
                        prop.StringValue = value.ToString();
                        break;
                }
            }
            else
            {
                Properties ??= new List<Property>();

                switch (type)
                {
                    case PropertyType.Boolean:
                        Properties.Add(new Property
                        {
                            Name = name,
                            Type = type,
                            BooleanValue = (bool) value
                        });
                        break;
                    case PropertyType.Number:
                        Properties.Add(new Property
                        {
                            Name = name,
                            Type = PropertyType.Number,
                            NumberValue = Convert.ToDouble(value)
                        });
                        break;
                    case PropertyType.DateTime:
                        Properties.Add(new Property
                        {
                            Name = name,
                            Type = PropertyType.DateTime,
                            DateTimeValue = (DateTime) value
                        });
                        break;
                    case PropertyType.String:
                        Properties.Add(new Property
                        {
                            Name = name,
                            Type = PropertyType.String,
                            StringValue = value.ToString()
                        });
                        break;
                    default:
                        Properties.Add(new Property
                        {
                            Name = name,
                            Type = PropertyType.String,
                            StringValue = value.ToString()
                        });
                        break;
                }
            }
        }

        /// <summary>
        ///     Merges a dictionary of values into a given property group.  For each key in the dictionary:
        ///     1.  If there is currently no property with the name of the key, then it's created and added
        ///     2.  If there is an existing property with a name matching the key, then it's value is replaced.
        /// </summary>
        /// <param name="source">A dictionary containing a series of 1-level KV pairs to merge into the property group</param>
        public void MergeDictionary(Dictionary<string, object>? source)
        {
            if (source != null)
            {
                foreach (var key in source.Keys)
                {
                    var value = source[key];
                    switch (value)
                    {
                        case string s:
                            AddOrReplaceProperty(key, PropertyType.String, value);
                            break;
                        case bool b:
                            AddOrReplaceProperty(key, PropertyType.Boolean, value);
                            break;
                        case int i:
                            AddOrReplaceProperty(key, PropertyType.Number, value);
                            break;
                        case float f:
                            AddOrReplaceProperty(key, PropertyType.Number, value);
                            break;
                        case long l:
                            AddOrReplaceProperty(key, PropertyType.Number, value);
                            break;
                        case DateTime dt:
                            AddOrReplaceProperty(key, PropertyType.DateTime, value);
                            break;
                        default:
                            AddOrReplaceProperty(key, PropertyType.String, value);
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///     Helper function for searching for a given property
        /// </summary>
        /// <param name="propertyName">The name of the property to check for</param>
        /// <returns>true if the property exists within the property group</returns>
        public bool HasProperty(string propertyName)
        {
            return Properties != null && Properties.Any(p => p.Name.Equals(propertyName));
        }

        /// <summary>
        ///     Retrieves a given property by name
        /// </summary>
        /// <param name="propertyName">The name of the property to try and return</param>
        /// <returns>The property or null</returns>
        public Property? GetPropertyByName(string propertyName)
        {
            return HasProperty(propertyName) ? Properties?.First(p => p.Name.Equals(propertyName)) : null;
        }
    }
}