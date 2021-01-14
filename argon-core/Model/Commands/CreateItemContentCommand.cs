using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace JCS.Argon.Model.Commands
{
    public class CreateItemContentCommand
    {
        public Dictionary<string, object> Properties { get; set; } = null!;

        public IFormFile File { get; set; } = null!;
    }
}