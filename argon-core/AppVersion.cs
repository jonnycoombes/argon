namespace JCS.Argon
{
    public class AppVersion
    {
        public static int Major { get; }= 0;

        public static int Minor { get;  } = 1;

        public static int Patch { get; } = 2;

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}