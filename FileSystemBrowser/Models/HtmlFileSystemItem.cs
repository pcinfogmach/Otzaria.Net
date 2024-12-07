using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSystemBrowser.Models
{
    public class HtmlFileSystemItem : FileSystemItem
    {
        public bool IsTagsLoaded { get; set; }

        public HtmlFileSystemItem(string rootDirectory, string path, bool isDirectory, FileSystemItem parent, string name = "", int level = 0)
            : base(rootDirectory, path, isDirectory, parent, name)
        {
            Level = level;
        }

        public async Task LoadTags(string rootDirectory)
        {
            if (IsTagsLoaded) return;
            IsTagsLoaded = true;
            Children = await Task.Run(() =>
            {
                var headerTags = new ObservableCollection<FileSystemItem>();
                try
                {
                    string content = System.IO.File.ReadAllText(Path);
                    string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    HtmlFileSystemItem htmlParent = this;
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

                            if (name == this.Name) continue;

                            while (htmlParent.Parent is HtmlFileSystemItem htmlGrandParent && level <= htmlParent.Level)
                                htmlParent = htmlGrandParent;

                            if (htmlParent is HtmlFileSystemItem htmlItem)
                                name = $"{htmlItem.Name} {name}";

                            var newTagItem = new HtmlFileSystemItem(rootDirectory, Path, false, htmlParent, name, level);
                            if (htmlParent == this)
                                headerTags.Add(newTagItem);
                            else
                                htmlParent.Children.Add(newTagItem);
                            htmlParent = newTagItem;
                        }
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }

                return headerTags;
            });
        }
    }
}
