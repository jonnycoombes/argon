namespace JCS.Argon.Model.Commands
{
    /// <summary>
    ///     Command for the patching of existing collections - TODO needs to have constraint support added
    /// </summary>
    public class PatchCollectionCommand
    {
        /// <summary>
        ///     Updated name for a collection - must be unique
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Updated description for a collection
        /// </summary>
        public string? Description { get; set; }
    }
}