using Otzaria.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Otzaria.Net.Models
{
    public class PdfFileSystemItem : FileSystemItem
    {
        public PdfFileSystemItem(string path, FileSystemItem parent = null) : base(path, parent) { }
    }

    public class FileSystemItem : ViewModelBase
    {
        public string Name { get; set; }
        public string Extension { get; }
        public FileSystemItem Parent { get; }
        public string FullPath { get; }
        public string ParentDirectory { get; }

        protected bool _isExpanded;
        public virtual bool IsExpanded { get => _isExpanded; set => SetField(ref _isExpanded, value); }

        public FileSystemItem(string path, FileSystemItem parent = null)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Extension = Path.GetExtension(path);
            FullPath = path;

            string pattern = @"(C:\\|אוצריא\\|otzaria)";
            ParentDirectory = Path.GetDirectoryName(path);
            ParentDirectory = Regex.Replace(ParentDirectory, pattern, "", RegexOptions.IgnoreCase);
            ParentDirectory = ParentDirectory.Replace("\\", " \\ "); // Escape backslashes for the final transformation

            Parent = parent;
        }

        public override string ToString() => Name;
    }
}
