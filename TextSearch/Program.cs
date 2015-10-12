using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextSearch
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Input parameters are invalid.\n");
                return;
            }
            var needle = args[2];

            var files = FindFiles(args[0], args[1]);

            foreach (var file in files)
            {
                var offset = SearchInFile(file, needle);

                if (offset == -1) continue;

                Console.WriteLine(file.FullName);
                Console.Write("Found text position: ");
                Console.WriteLine(offset);
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static IEnumerable<FileInfo> FindFiles(string path, string pattern)
        {
            return new DirectoryInfo(path).GetFiles(pattern, SearchOption.AllDirectories);
        }


        private static int SearchInFile(FileSystemInfo file, string needle)
        {
            var byteBuffer = File.ReadAllBytes(file.FullName);

            var enc = ReadBOM(byteBuffer);

            if (!Equals(enc, Encoding.ASCII)) // if BOM is detected
            {
                var stringBuffer = enc.GetString(File.ReadAllBytes(file.FullName));
                var offset = stringBuffer.IndexOf(needle, StringComparison.InvariantCulture);
                
                return offset;
            }

            var encoList = new List<Encoding> // if not - search by all ways
            {
                Encoding.Default,
                Encoding.ASCII,
                Encoding.UTF8,
                Encoding.Unicode,
                Encoding.BigEndianUnicode
            };
            
            foreach (var enco in encoList)
            {
                var offset = enco.GetString(byteBuffer).IndexOf(needle, StringComparison.InvariantCulture);
                if (offset == -1) continue;
                return offset;
            }   
            return -1;  
        }

        private static Encoding ReadBOM(byte[] bom)
        {
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode;
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode;
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;

        }
    }
}
