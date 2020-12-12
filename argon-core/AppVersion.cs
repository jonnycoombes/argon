namespace JCS.Argon
{
    public class AppVersion
    {
        /// <summary>
        /// Application major version number
        /// </summary>
        public static int Major { get; }= 1;

        /// <summary>
        /// Application minor version
        /// </summary>
        public static int Minor { get;  } = 0;

        /// <summary>
        /// Application patch/build level
        /// </summary>
        public static int Build { get; } = 4;

        /// <summary>
        /// Schema major version
        /// </summary>
        public static int SchemaMajor { get; } = 1;

        /// <summary>
        /// Schema minor version
        /// </summary>
        public static int SchemaMinor { get; } = 0;

        /// <summary>
        /// Schema patch/build number
        /// </summary>
        public static int SchemaBuild { get; } = 3;

        /// <summary>
        /// Returns the current internal version number
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}";
        }

        /// <summary>
        /// Returns the current schema version number - should correlate to the currently active migration
        /// level
        /// </summary>
        /// <returns></returns>
        public string ToStringSchema()
        {
            return $"{SchemaMajor}.{SchemaMinor}.{SchemaBuild}";
        }
    }
}
