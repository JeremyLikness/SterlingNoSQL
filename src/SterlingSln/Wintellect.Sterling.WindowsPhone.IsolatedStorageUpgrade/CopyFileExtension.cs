using System.IO;
using System.IO.IsolatedStorage;

namespace Wintellect.Sterling.IsolatedStorageUpgrade
{
    public static class CopyFileExtension
    {
        public static void CopyFile(this IsolatedStorageFile iso, string src, string target, bool overwrite)
        {
            using (var srcFile = iso.OpenFile(src, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(srcFile))
                {
                    var bytes = new byte[srcFile.Length];
                    br.Read(bytes, 0, bytes.Length);
                    using (var tgtFile = new BinaryWriter(iso.OpenFile(target, FileMode.Create, FileAccess.Write)))
                    {
                        tgtFile.Write(bytes);
                    }
                }
            }            
        }
    }
}