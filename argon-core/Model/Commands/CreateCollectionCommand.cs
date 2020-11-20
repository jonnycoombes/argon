namespace JCS.Argon.Model.Commands
{
    public class CreateCollectionCommand
    {
        public CreateCollectionCommand(string name, string? description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }
        
        public string? Description { get; set; }

    }
}