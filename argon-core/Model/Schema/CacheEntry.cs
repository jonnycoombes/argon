#region

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#endregion

namespace JCS.Argon.Model.Schema
{
    /// <summary>
    ///     Enumeration of available cache types
    /// </summary>
    public enum CacheEntryType
    {
        String,
        Json,
        Long,
        Integer,
        DateTime
    }

    /// <summary>
    ///     Class representing a single cache entry
    /// </summary>
    [Table("cache", Schema = "argon")]
    public class CacheEntry
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
        public Guid? Id { get; set; }

        /// <summary>
        ///     A partition identiier, allows for the grouping and subsetting of cache entries
        /// </summary>
        [Required]
        [Column(TypeName = "varchar(512)")]
        public string Partition { get; set; } = null!;

        /// <summary>
        ///     The name of the property
        /// </summary>
        [Required]
        [Column(TypeName = "varchar(512)")]
        public string Key { get; set; } = null!;

        /// <summary>
        ///     The type of the property
        /// </summary>
        [Required]
        public CacheEntryType Type { get; set; }

        /// <summary>
        ///     Optional string value
        /// </summary>
        [Column(TypeName = "varchar(512)")]
        public string? StringValue { get; set; } = null!;

        /// <summary>
        ///     Optional long value
        /// </summary>
        public long? LongValue { get; set; }

        /// <summary>
        ///     Optional int value
        /// </summary>
        public int? IntValue { get; set; }

        /// <summary>
        ///     Optional datetime value
        /// </summary>
        public DateTime? DateTimeValue { get; set; }
    }
}