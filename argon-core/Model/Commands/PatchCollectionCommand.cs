namespace JCS.Argon.Model.Commands
{
    public class PatchCollectionCommand
    {
        /// <summary>
        /// Updated name for a collection - must be unique
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Updated description for a collection
        /// </summary>
        public string? Description { get; set; }
    }
}