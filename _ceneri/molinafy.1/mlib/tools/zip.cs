using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;

namespace mlib
{
    public class zip
    {
        public static void zip_folder(string in_dir, string out_file)
        {
            string[] sFiles = Directory.GetFiles(in_dir, "*.*", SearchOption.AllDirectories);
            int iDirLen = in_dir[in_dir.Length - 1] == Path.DirectorySeparatorChar ? in_dir.Length : in_dir.Length + 1;

            using (FileStream outFile = new FileStream(out_file, FileMode.Create, FileAccess.Write, FileShare.None))
            using (GZipStream str = new GZipStream(outFile, CompressionMode.Compress))
                foreach (string sFilePath in sFiles)
                    zipFile(in_dir, sFilePath.Substring(iDirLen), str);
        }

        public static void unzip(string zip_file, string out_folder, string likes = "")
        {
            using (FileStream inFile = new FileStream(zip_file, FileMode.Open, FileAccess.Read, FileShare.None))
            using (GZipStream zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
                while (unzipFile(out_folder, zipStream, likes)) ;
        }

        #region internals

        protected static void zipFile(string sDir, string sRelativePath, GZipStream zipStream)
        {
            //Compress file name
            char[] chars = sRelativePath.ToCharArray();
            zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
            foreach (char c in chars)
                zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

            //Compress file content
            byte[] bytes = File.ReadAllBytes(Path.Combine(sDir, sRelativePath));
            zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
            zipStream.Write(bytes, 0, bytes.Length);
        }

        protected static bool unzipFile(string sDir, GZipStream zipStream, string likes = ""/*, ProgressDelegate progress = null*/)
        {
            //Decompress file name
            byte[] bytes = new byte[sizeof(int)];
            int Readed = zipStream.Read(bytes, 0, sizeof(int));
            if (Readed < sizeof(int))
                return false;

            int iNameLen = BitConverter.ToInt32(bytes, 0);
            bytes = new byte[sizeof(char)];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < iNameLen; i++)
            {
                zipStream.Read(bytes, 0, sizeof(char));
                char c = BitConverter.ToChar(bytes, 0);
                sb.Append(c);
            }
            string sFileName = sb.ToString();
            //if (progress != null)
            //    progress(sFileName);

            //Decompress file content
            bytes = new byte[sizeof(int)];
            zipStream.Read(bytes, 0, sizeof(int));
            int iFileLen = BitConverter.ToInt32(bytes, 0);

            bytes = new byte[iFileLen];
            zipStream.Read(bytes, 0, bytes.Length);

            if (likes != "")
            {
                string[] splitted = likes.Split(';');
                foreach (string split in splitted)
                    if (!strings.like(sFileName, split)) return true;
            }

            string sFilePath = Path.Combine(sDir, sFileName);
            string sFinalDir = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(sFinalDir))
                Directory.CreateDirectory(sFinalDir);

            using (FileStream outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                outFile.Write(bytes, 0, iFileLen);

            return true;
        }

        #endregion
    }
}


 
