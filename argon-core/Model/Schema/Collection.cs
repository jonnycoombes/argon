#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#endregion

#pragma warning disable 8618

namespace JCS.Argon.Model.Schema
{
    /// <summary>
    ///     The main collection entity.  This represents the top of the storage heirarchy
    /// </summary>
    [Table("collection", Schema = "argon")]
    public class Collection
    {
        /// <summary>
        ///     An enumeration of "stock" provider properties - returned in the property bag for certain operations
        /// </summary>
        public enum StockCollectionProperties
        {
            Path,
            CreateDate,
            LastAccessed,
            Length,
            ContentType
        }

        /// <summary>
        ///     The primary concurrency token for this entity
        /// </summary>
        [JsonIgnore]
        [Timestamp]
        public byte[]? Timestamp { get; set; }

        /// <summary>
        ///     The unique identifier for the collection
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid? Id { get; set; }

        /// <summary>
        ///     The name of the collection
        /// </summary>
        [Required]
        [Column(TypeName = "varchar(512)")]
        public string Name { get; set; } = null!;

        /// <summary>
        ///     The description for the collection (optional)
        /// </summary>
        [Column(TypeName = "varchar(512)")]
        public string? Description { get; set; }

        /// <summary>
        ///     The length of the collection in terms of number of collection items
        /// </summary>
        public long NumberOfItems { get; set; } = 0;

        /// <summary>
        ///     The aggregate size of the collection in bytes
        /// </summary>
        public long TotalSizeBytes { get; set; } = 0;

        /// <summary>
        ///     The tag associated with the VSP provider for this collection
        /// </summary>
        [Column(TypeName = "varchar(512)")]
        public string ProviderTag { get; set; }

        /// <summary>
        ///     The items associated with this collection
        /// </summary>
        [JsonIgnore]
        public List<Item> Items { get; set; } = null!;

        /// <summary>
        /// </summary>
        public PropertyGroup? PropertyGroup { get; set; }

        /// <summary>
        /// </summary>
        public ConstraintGroup? ConstraintGroup { get; set; }
    }
}