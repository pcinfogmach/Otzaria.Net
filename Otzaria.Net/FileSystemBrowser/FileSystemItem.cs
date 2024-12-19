using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace FileSystemBrowser
{
    public class FileSystemItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Tags { get; set; } = "";
        public bool IsFile { get; set; }
        public int Level { get; set; }
        public int Index { get; set; } // for generic usage
        public FileSystemItem Parent;
        public List<FileSystemItem> Children = new List<FileSystemItem>();

        public override string ToString() => Name;
        public string ExtendedName() => Path + " " + Name;
        public string Extension => System.IO.Path.GetExtension(Path);
        public bool HasChildren => Children.Count > 0;

        public FileSystemItem(string rootDirectory, string path, bool isFile, int level = short.MinValue, FileSystemItem parent = null, string name = "", int index = 0)
        {
            if (string.IsNullOrEmpty(path)) return;

            Path = path;
            IsFile = isFile;
            Level = level;
            Parent = parent;
            Index = index;

            Name = string.IsNullOrEmpty(name) ? System.IO.Path.GetFileNameWithoutExtension(path) : name;
            Name = FileSystemItemHelper.CleanNonWordChars(Name);

            Tags = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetExtension(path))
                .Replace(rootDirectory, "").Trim('\\').Replace("\\", " \\ ");

            if (!isFile) LoadChildren(rootDirectory);
            Index = index;
        }

        void LoadChildren(string rootDirectory)
        {
            int i = Index;
            var directories = Directory.GetDirectories(Path)
                .OrderBy(dir =>
                {
                    var index = Array.IndexOf(FileSystemItemHelper.directoryOrder, System.IO.Path.GetFileName(dir));
                    return index == -1 ? int.MaxValue : index; // Unmatched items go to the end
                });

            foreach (var directory in directories)
                Children.Add(new FileSystemItem(rootDirectory, directory, false, Level + 1, this));

            var files = Directory.GetFiles(Path)
                .OrderBy(file =>
                {
                    var index = Array.IndexOf(FileSystemItemHelper.fileOrder, System.IO.Path.GetFileNameWithoutExtension(file));
                    return index == -1 ? int.MaxValue : index; // Unmatched items go to the end
                });

            foreach (var file in files)
                Children.Add(new FileSystemItem(rootDirectory, file, true, 0, this));
        }

        public IEnumerable<FileSystemItem> EnumerateChildrenRecursive()
        {
            foreach (var child in Children)
            {
                yield return child;

                foreach (var item in child.EnumerateChildrenRecursive())
                    yield return item;
            }
        }

        public List<FileSystemItem> LevelOrderTraversal()
        {
            List<FileSystemItem> results = new List<FileSystemItem>();
            List<FileSystemItem> currentLevel = new List<FileSystemItem>(this.Children);

            while (currentLevel.Count > 0)
            {
                results.AddRange(currentLevel);
                List<FileSystemItem> nextLevel = new List<FileSystemItem>();

                foreach (var item in currentLevel)
                {
                    if (item.Children.Count > 0)
                        nextLevel.AddRange(item.Children);
                }

                currentLevel = nextLevel;
            }

            return results;
        }
    }
}
