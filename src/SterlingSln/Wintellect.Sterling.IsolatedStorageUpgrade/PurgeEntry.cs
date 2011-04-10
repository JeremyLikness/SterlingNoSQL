namespace Wintellect.Sterling.IsolatedStorageUpgrade
{
    public class PurgeEntry
    {
        public bool IsDirectory { get; set; }
        public string Path { get; set; }

        public static PurgeEntry CreateEntry(bool isDirectory, string path)
        {
            return new PurgeEntry
                       {
                           IsDirectory = isDirectory,
                           Path = path
                       };
        }
    }
}