namespace JCS.Argon
{
    public static class AppVersion
    {
        public static int Major { get; }= 0;

        public static int Minor { get;  } = 1;

        public static int Patch { get; } = 1;

        public static string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}