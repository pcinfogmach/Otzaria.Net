using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemBrowser
{
    public class HtmlFileSystemItem : FileSystemItem
    {
        public bool IsTagsLoaded { get; set; }
        public int Index { get; set; }
        private CancellationTokenSource _cancellationTokenSource;

        public HtmlFileSystemItem(string rootDirectory, string path, bool isDirectory, int level, FileSystemItem parent = null, string name = "", int index = 0)
            : base(rootDirectory, path, isDirectory, level, parent, name)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            this.Index = index;
        }

        public void CancelLoadingTags()
        {
            if (!IsTagsLoaded) return;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            Children.Clear();
            IsTagsLoaded = false;
        }

        public async Task<string> LoadContent(string rootDirectory, bool loadTagsOnly, bool useFullName)
        {
            if (IsTagsLoaded) return string.Empty;
            IsTagsLoaded = true;

            StringBuilder stringBuilder = new StringBuilder();
            Children = await Task.Run(() =>
            {
                var headerTags = new ObservableCollection<FileSystemItem>();
                try
                {
                    using (var reader = new StreamReader(Path))
                    {
                        HtmlFileSystemItem htmlParent = this;
                        int index = 0;
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

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

                                if (useFullName || htmlParent != this)
                                    name = $"{htmlParent.Name} {name}";

                                var newTagItem = new HtmlFileSystemItem(rootDirectory, Path, false, level, htmlParent, name, ++index);

                                if (htmlParent == this)
                                    headerTags.Add(newTagItem);
                                else
                                    htmlParent.Children.Add(newTagItem);

                                htmlParent = newTagItem;
                            }

                            if(!loadTagsOnly)
                                stringBuilder.Append($"<line>{line} </line>");
                        }

                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return headerTags;
            }, _cancellationTokenSource.Token);

            return stringBuilder.ToString();
        }
    }
}
