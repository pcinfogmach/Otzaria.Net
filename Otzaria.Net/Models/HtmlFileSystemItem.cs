using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Otzaria.Net.Models
{
    public class HtmlTagItem : HtmlFileSystemItem
    {
        string _htmlHeaderId;
        public string HtmlHeaderId { get => _htmlHeaderId; set => SetField(ref _htmlHeaderId, value); }
        public override bool IsExpanded {  get => _isExpanded;  set => SetField(ref _isExpanded, value); }

        public HtmlTagItem(string path, string name, string htmlId = "",  FileSystemItem parent = null, int level = -1) : base(path, parent)
        {
            Level = level;
            Name = name;
            HtmlHeaderId = htmlId;
        }
    }

    public class HtmlFileSystemItem : FileSystemItem
    {
        public int Level { get; set; } = 0;
        ObservableCollection<FileSystemItem> _htmlTags = new ObservableCollection<FileSystemItem>() { };
        ObservableCollection<FileSystemItem> _searchResultsTags = new ObservableCollection<FileSystemItem>() { };
        public ObservableCollection<FileSystemItem> HtmlTags { get => _htmlTags; set => SetField(ref _htmlTags, value); }
        public ObservableCollection<FileSystemItem> SearchResultsTags { get => _htmlTags; set => SetField(ref _htmlTags, value); }

        public override bool IsExpanded 
        {
            get => _isExpanded;
            set 
            {
                SetField(ref _isExpanded, value);
                LoadTags();
            }
        }

        public HtmlFileSystemItem(string path, FileSystemItem parent) : base (path, parent) 
        {
            LoadTags(); 
        }

        private async void LoadTags()
        {
            if (!IsExpanded)
            {
                if (this is HtmlTagItem) { }
                else HtmlTags = new ObservableCollection<FileSystemItem>() { new FileSystemItem("מחפש כותרות במסמך...") };
                return;
            }

            HtmlTags = await Task.Run(() =>
            {
                var headerTags = new ObservableCollection<FileSystemItem>();
                try
                {
                    string content = File.ReadAllText(FullPath);
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

                            while (htmlParent.Parent is HtmlFileSystemItem htmlGrandParent && level <= htmlParent.Level)
                                htmlParent = htmlGrandParent;

                            string id;
                            if (htmlParent is HtmlTagItem htmlTagItem)
                                id = $"{htmlTagItem.HtmlHeaderId}.{name}";
                            else id = name;

                            if (id == this.Name) continue;
                            var newTagItem = new HtmlTagItem(FullPath, name, id, htmlParent, level) { };
                            if (htmlParent == this) 
                                headerTags.Add(newTagItem);
                            else
                                htmlParent.HtmlTags.Add(newTagItem);
                            htmlParent = newTagItem;
                        }
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }

                return headerTags;
            });
        }

        //public async Task LoadSearchResultsTags(string searchPattern)
        //{
        //    if (string.IsNullOrEmpty(searchPattern)) return;
        //    HtmlHeaderTags = await Task.Run(()=> 
        //    {
        //        var headerTags = new ObservableCollection<FileSystemItem>();

        //        try
        //        {
        //            string[] splitPattern = searchPattern.Split(new char[] { ' ', ',', }, StringSplitOptions.RemoveEmptyEntries);
        //            string content = File.ReadAllText(FullPath);
        //            string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        //            Stack<string> tagStack = new Stack<string>();
        //            foreach (var line in lines)
        //            {
        //                Match regexMatch = Regex.Match(line.ToLower().Trim(), @"^(<h([1-6])>||\(([^(]+)\))([^<(]+)");
        //                if (regexMatch.Success)
        //                {
        //                    string level = regexMatch.Groups[2].ToString() ?? regexMatch.Groups[3].ToString();
        //                    if (int.TryParse(level, out int l)) Level = l;
        //                }
        //                else { Level = 7; }

        //                string id = regexMatch.Groups[4].ToString();
        //                if (id.Length > 20) id = id.Substring(0, 20) + "...";

        //                try
        //                {
        //                    while (Level >= tagStack.Count) tagStack.Pop();
        //                    id = $"{tagStack.Peek()}.{id}";
        //                }
        //                catch { }

        //                if (IsSearchPatternMatch(splitPattern, line))
        //                {
        //                    headerTags.Add(new HtmlFileSystemItem(id, FullPath, this) { });
        //                }
        //            }
        //        }
        //        catch (Exception ex) { Debug.WriteLine(ex.Message); }

        //        return headerTags;
        //    });
        //}


        bool IsSearchPatternMatch(string[] searchPattern, string header)
        {
            string[] splitHeaders = header.Split(new char[] { ' ', ',', '>', '<' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string searchTerm in searchPattern)
            {
                if (!splitHeaders.Contains(searchTerm)) return false;
            }
            return true;
        }
    }
}
