using FileSystemBrowser.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace FileSystemBrowser.Models
{
    public class FileSystemItem
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Tags { get; set; }
        public int Level { get; set; } = 0;
        public bool IsDirectory { get; set; }
        public ObservableCollection<FileSystemItem> Children { get; set; } = new ObservableCollection<FileSystemItem>();

        public FileSystemItem(string rootDirectory, string path, bool isDirectory)
        {
            FilePath = path;
            Name = Path.GetFileNameWithoutExtension(path);
            Tags = Path.Combine(Path.GetDirectoryName(path), Path.GetExtension(path))
            .Replace(rootDirectory, "")
                .Trim('\\')
                .Replace("\\", " \\ ");
            IsDirectory = isDirectory;


        }

        public override string ToString() => Name;

        public ICommand LoadChildrenCommand { get; }
        LoadChildrenCommand = new RelayCommand(LoadChildrenFromHtml);

        public async void LoadChildrenFromHtml()
        {
            if (!File.Exists(FilePath)) { MessageBox.Show($"File not found: {FilePath}"); return; }

            Children = await Task.Run(() =>
            {
                var headerTags = new List<FileSystemItem>();
                try
                {
                    string content = File.ReadAllText(FilePath);
                    string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    Stack<FileSystemItem> ParentStack = new Stack<FileSystemItem>();
                    foreach (var line in lines)
                    {
                        Match regexMatch = Regex.Match(line.ToLower().Trim(), @"^(\(([^( ]+)\)|<h([1-6])>)([^<(]+)");
                        if (regexMatch.Success)
                        {
                            int level = 1;
                            string levelString = regexMatch.Groups[3].ToString();
                            if (int.TryParse(levelString, out int L)) level = L;

                            string name = regexMatch.Groups[4].ToString();

                            if (line.Trim().StartsWith("("))
                            {
                                level = 7;
                                name = regexMatch.Groups[2].ToString();
                            }

                            if (name == Path.GetFileNameWithoutExtension(FilePath)) continue;

                            while (ParentStack.Peek().Level <= level)
                                ParentStack.Pop();

                            name = $"{ParentStack.Peek().Name} {name}";

                            var newItem = new FileSystemItem("", name, false) { Level = level };
                            ParentStack.Push(newItem);
                            headerTags.Add(newItem);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                return new ObservableCollection<FileSystemItem>();
            });
        }
    }
}
