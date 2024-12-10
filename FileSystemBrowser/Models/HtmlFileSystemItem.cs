using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemBrowser.Models
{
    public class HtmlFileSystemItem : FileSystemItem
    {
        public bool IsTagsLoaded { get; set; }
        private CancellationTokenSource _cancellationTokenSource;

        public HtmlFileSystemItem(string rootDirectory, string path, bool isDirectory, int level, FileSystemItem parent, string name = "")
            : base(rootDirectory, path, isDirectory, level, parent, name)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void CancelLoadingTags()
        {
            if (!IsTagsLoaded) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            Children = new ObservableCollection<FileSystemItem>();
            IsTagsLoaded = false;
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
                    if (Path.Contains("דברי חמודות"))
                    {
                        Console.Write("");
                    }
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

                            if (line.StartsWith("("))
                            {
                                level = 7;
                                name = regexMatch.Groups[2].ToString();
                            }

                            if (name == this.Name) continue;

                            while (htmlParent.Parent is HtmlFileSystemItem htmlGrandParent && level <= htmlParent.Level)
                                htmlParent = htmlGrandParent;

                            name = $"{htmlParent.Name} {name}";

                            var newTagItem = new HtmlFileSystemItem(rootDirectory, Path, false, level, htmlParent, name);
                            
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
            }, _cancellationTokenSource.Token);
        }
    }
}
