using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NoteEditor.Core.IO
{
    public class NoteLoader
    {
        public static bool Equal(string filepath, ref string content)
        {
            if(filepath == string.Empty || !File.Exists(filepath)) return false;

            FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.Read);
            MD5 mD5 = MD5.Create();
            byte[] fileHash    = mD5.ComputeHash(fs);
            byte[] contentHash = mD5.ComputeHash(Encoding.UTF8.GetBytes(content));
            fs.Flush();
            fs.Close();
            return Enumerable.SequenceEqual(fileHash, contentHash);
        }

        public static void Save(string srcFile, string dstFile)
        {
            File.Copy(srcFile, dstFile);
        }

        public static void Save(string filepath, ref string content)
        {
            if (string.IsNullOrEmpty(filepath) || content == null) return;

            string directory = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            FileStream fs = new FileStream(filepath, FileMode.Create);

            fs.Write(Encoding.UTF8.GetBytes(content));
            fs.Flush();
            fs.Close();
        }

        public static bool IsFolder(string path)
        {
            return Directory.Exists(path);
        }

        public static string Load(string filepath)
        {
            if (!File.Exists(filepath))
            {
                return "";
            }
            using(StreamReader sr = new StreamReader(filepath, new FileStreamOptions() { Mode = FileMode.Open }))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(sr.ReadToEnd());
                return stringBuilder.ToString();
            }
        }
    }
}
