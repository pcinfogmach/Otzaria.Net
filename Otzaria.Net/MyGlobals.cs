using FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otzaria.Net
{
    public static class Globals
    {
        public static FileSystemItem RootItem { get; set; }

        public static void InitializeRoot(string path)
        {
            if (RootItem == null)
            {
                bool isFile = File.Exists(path);
                string extension = Path.GetExtension(path);
                int level = isFile ? 0 : short.MinValue;

                RootItem = new FileSystemItem(path, path, isFile);
            }
        }
    }
}
