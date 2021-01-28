using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JCS.Argon.Model.Schema
{
    [Table("itemVersion", Schema = "argon")]
    public class ItemVersion
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
        /// The major version number 
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// The minor version number
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// The name of the version
        /// </summary>
        [Required]
        [Column(TypeName = "varchar(512)")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// The size of this version in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// The MIME type for the version
        /// </summary>
        [Column(TypeName = "varchar(512)")]
        public string? MIMEType { get; set; }

        /// <summary>
        /// The unique thumbrpint for this version
        /// </summary>
        [Column(TypeName = "varbinary(1024)")]
        public byte[]? Thumbprint { get; set; }

        /// <summary>
        /// The parent <see cref="Item"/>
        /// </summary>
        [Required]
        [JsonIgnore]
        public Item Item { get; set; } = null!;
    }
}