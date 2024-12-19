using FileSystemBrowser;
using MyModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Otzaria.Net.FileViewer
{
    public class LinksViewModel : ViewModelBase
    {
        private string _path;
        static string Name { get; set; }
        private ObservableCollection<CheckedTreeViewItemModelBase> _commentryFilters;
        private ObservableCollection<LinkItem> _links = new ObservableCollection<LinkItem>() { new LinkItem { heRef_2 = "אנא לחץ על שורה במסמך לטעינת הקישורים"} };
        static List<string> _groupingEnum;
        static List<string> linksOrder = new List<string>(6) { "mishnah", "mishnah in talmud", "mesorat hashas", "ein mishpat / ner mitsvah", "midrash" };

        public ObservableCollection<CheckedTreeViewItemModelBase> CommentryFilters
        {
            get => _commentryFilters;
            set => SetProperty(ref _commentryFilters, value);
        }

        public string Path
        {
            get => _path;
            set {  if (SetProperty(ref _path, value)) LoadCommentryFilters(); }
        }


        public ObservableCollection<LinkItem> Links
        {
            get => _links;
            set
            {
                if (value == null || value.Count <= 0)
                    value = new ObservableCollection<LinkItem> { new LinkItem { heRef_2 = "לא נמצאו קישורים, אנא בחר קטע אחר במסמך." } };
                SetProperty(ref _links, value);
            }
        }

        private async void LoadCommentryFilters()
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            _groupingEnum = new List<string> { "תרגומים", "מדרש", "גאונים", "ראשונים", "אחרונים", "מחברי זמננו", "מפרשים", "תלמוד", "תוספתא", "קבלה", $"על {Name}", "אחר", };
            List<CheckedTreeViewItemModelBase> linkGroups = new List<CheckedTreeViewItemModelBase>();

            try
            {
                var links = await GetLinksCollection(new CancellationToken());

                var groupedLinks = links
                    .Where(l => l.Conection_Type == "commentary" || l.Conection_Type == "targum" || l.Conection_Type == "footnotes")
                    .GroupBy(link => link.Name)
                    .Select(chunk => chunk.First())
                    .GroupBy(link => link.GroupName);

                foreach (var group in groupedLinks)
                {
                    linkGroups.Add(new CheckedTreeViewItemModelBase
                    {
                        Name = group.Key,
                        Children = new ObservableCollection<CheckedTreeViewItemModelBase>(group.ToList())
                    });
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Task Cancelled");
            }

            CommentryFilters = new ObservableCollection<CheckedTreeViewItemModelBase>(linkGroups);
        }

        public async Task<string> LoadCommentries(double line_index_1, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                var stringBuilder = new StringBuilder();
                var commentryList = await GetCommentryLinks(line_index_1 + 2, cancellationToken);

                foreach (var commentry in commentryList)
                {
                    cancellationToken.ThrowIfCancellationRequested(); // Check cancellation

                    string content = await GetCommentryLine(commentry.path_2, line_index_1, cancellationToken);
                    if (!string.IsNullOrEmpty(content))
                    {
                        stringBuilder.AppendLine($"<header>{commentry.Name}</header>");
                        stringBuilder.AppendLine(content);
                    }
                }

                string result = stringBuilder.ToString();
                return !string.IsNullOrWhiteSpace(result) ? result : "לא נמצאו מפרשים עבור שורה זו";
            });
        }

        public async Task LoadLinks(double line_index_1, CancellationToken cancellationToken)
        {
            Links = new ObservableCollection<LinkItem> { new LinkItem() { heRef_2 = "טוען קישורים..."} };
            Links = await Task.Run(()=> GetExternalLinks(line_index_1, cancellationToken));
        }

        async Task<string> GetCommentryLine(string path, double line_index_1, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path)) return null;

            path = path.Replace("אוצריא\\", "");
            path = System.IO.Path.Combine(Globals.RootItem.Path, path);
            if (!File.Exists(path)) { path = new FileSystemLocater(Globals.RootItem).WordBasedSearch(path).Path; }
            if (!File.Exists(path)) return null;

            using (var reader = new StreamReader(path))
            {
                int currentIndex = 0;
                while (!reader.EndOfStream && currentIndex < line_index_1)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    reader.ReadLine();
                    currentIndex++;
                }

                cancellationToken.ThrowIfCancellationRequested();
                return reader.ReadLine();
            }
        }

        async Task<IOrderedEnumerable<LinkItem>> GetCommentryLinks(double line_index_1, CancellationToken cancellationToken)
        {
            var links = await GetFilteredLinksCollection(line_index_1, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            //var checkedItems = _commentryFilters.SelectMany(e => e.GetAllCheckedChildren());
            return links.Where(l => (l.Conection_Type == "commentary"
                                    || l.Conection_Type == "targum"
                                    || l.Conection_Type == "footnotes"))
                        .OrderBy(link => _groupingEnum.IndexOf(link.GroupName));
        }

        async Task<ObservableCollection<LinkItem>> GetExternalLinks(double line_index_1, CancellationToken cancellationToken)
        {
            var links = await GetFilteredLinksCollection(line_index_1, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            return new ObservableCollection<LinkItem>(links
                        .Where(l => l.Conection_Type != "commentary"
                                    && l.Conection_Type != "targum"
                                    && l.Conection_Type != "footnotes")
                        .OrderBy(l => linksOrder.IndexOf(l.Conection_Type))
                        .ThenBy(l => l.ToString()));
        }

        async Task<IEnumerable<LinkItem>> GetFilteredLinksCollection(double line_index_1, CancellationToken cancellationToken)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(Path);
            string linksFilePath = System.IO.Path.Combine(@"C:\אוצריא\links", fileName + "_links.json");
            var links = await GetLinksCollection(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            return links.Where(l => l.line_index_1 == line_index_1);
        }

        async Task<IEnumerable<LinkItem>> GetLinksCollection(CancellationToken cancellationToken)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(Path);
            string linksFilePath = System.IO.Path.Combine(@"C:\אוצריא\links", fileName + "_links.json");

            if (File.Exists(linksFilePath))
            {
                using (var reader = new StreamReader(linksFilePath))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string json =  reader.ReadToEnd();
                    json = json.Replace("Conection Type", "Conection_Type");

                    cancellationToken.ThrowIfCancellationRequested();
                    var deserializedLinks = JsonSerializer.Deserialize<LinkItem[]>(json);
                    return deserializedLinks;
                }
            }

            return new List<LinkItem>();
        }

        public class LinkItem : CheckedTreeViewItemModelBase
        {
            public double line_index_1 { get; set; } // link source
            public string heRef_2 { get; set; } // = Name of link
            public string path_2 { get; set; }//  target file path
            public double line_index_2 { get; set; } // target line
            public string Conection_Type { get; set; }
            public string GroupName => _groupingEnum.FirstOrDefault(enumItem => path_2.Contains(enumItem)) ?? "אחר";

            public override string Name
            {
                get
                {
                    if (string.IsNullOrEmpty(_name))
                        _name = System.IO.Path.GetFileNameWithoutExtension(path_2);
                    return _name;

                }
                set => SetProperty(ref _name, value);
            }

            public override string ToString() => heRef_2;
        }
    }
}
