using System;

namespace JCS.Argon.Model.Schema
{
    public class Collection
    {
        /// <summary>
        /// The unique identifier for the collection
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The name of the collection
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The description for the collection (optional)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The length of the collection in terms of number of collection items
        /// </summary>
        public long Length { get; set; } = 0;

        /// <summary>
        /// The aggregate size of the collection in bytes
        /// </summary>
        public long Size { get; set; } = 0;
        
        

    }
}