namespace JCS.Argon.Model.Configuration
{
    public class ApiOptions
    {
        public const string ConfigurationSection = "argon";
        
        public VirtualStorageOptions VirtualStorageOptions { get; init; } = null!;
    }
}