#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;

#endregion

namespace JCS.Argon.Model.Commands
{
    /// <summary>
    ///     Command for the creation of new collection <see cref="Item" />s
    /// </summary>
    public class CreateItemContentCommand
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        private CreateItemContentCommand()
        {
        }

        /// <summary>
        ///     Properties for the new item
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = null!;

        /// <summary>
        ///     An <see cref="IFormFile" /> reference which can be used in order to retrieve the actual content for the item, along
        ///     with other
        ///     information such as MIME type etc...
        /// </summary>
        [Required(ErrorMessage = "You must specify content to be uploaded")]
        public IFormFile File { get; set; } = null!;
    }
}