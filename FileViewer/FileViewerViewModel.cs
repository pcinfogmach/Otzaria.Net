using MyHelpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer
{
    public class FileViewerViewModel : ViewModelBase
    {
        int _fontSize = 100;
        string _fontFamily = "Arial";
        bool _showFonts;
        ObservableCollection<FontFamily> _fontList;
        ObservableCollection<LinkItem> _links;
        ObservableCollection<LinkItem> _commentry;

        public string FilePath { get; set; }
        public string FontFamily { get => _fontFamily; set { SetProperty(ref _fontFamily, value); ShowFonts = false; } }
        public int FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }
        public bool ShowFonts { get => _showFonts; set => SetProperty(ref _showFonts, value); }
        public ObservableCollection<FontFamily> FontList { get { if (_fontList == null) PopulateFontList(); return _fontList; } set => SetProperty(ref _fontList, value); }
        public ObservableCollection<LinkItem> Links { get => _links; set => SetProperty(ref _links, value); }
        public ObservableCollection<LinkItem> Commentry { get => _commentry; set => SetProperty(ref _commentry, value); }

        public ICommand IncreaseFontSizeCommand { get; }
        public ICommand DecreaseFontSizeCommand { get; }

        public FileViewerViewModel()
        {
            IncreaseFontSizeCommand = new RelayCommand(() => FontSize += 5);
            DecreaseFontSizeCommand = new RelayCommand(() => FontSize -= 5);
        }

        void PopulateFontList()
        {
            string[] prioritizedFontNames = { "frankruehl", "times", "narkisim", "hadas", "calibri", "arial", "david", "gisha", "frank", "guttman", "aharoni", "hebrew", "rod", "miriam", "tahoma", "courier"};
            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(font =>
            {
                var index = prioritizedFontNames.Any(p => font.Source.ToLower().Contains(p)) ? 0 : 1;
                return index;
            }); ;
            _fontList = new ObservableCollection<FontFamily>(fontFamilies);
        }

        public void GetLinks(string line_index_1)
        {
            List<string> linksOrder = new List<string>(6) { "footnotes", "mishnah", "mishnah in talmud", "mesorat hashas", "ein mishpat / ner mitsvah", "midrash" };
            if (string.IsNullOrEmpty(line_index_1) || string.IsNullOrEmpty(FilePath)) return;
            string fileName = Path.GetFileName(FilePath);
            string linksFilePath = Path.Combine(@"C:\אוצריא\links", fileName + "_links.json");
            if(!File.Exists(linksFilePath)) return;

            var json = File.ReadAllText(linksFilePath).Replace("Conection Type", "Conection_Type");
            var links = JsonSerializer.Deserialize<LinkItem[]>(json);

            var currentLineLinks = links.Where(l => l.line_index_1.ToString() == line_index_1 + ".0");
            Links = new ObservableCollection<LinkItem>(currentLineLinks.Where(l => l.Conection_Type != "commentary" && l.Conection_Type != "targum").OrderBy(l => linksOrder.IndexOf(l.Conection_Type)).ThenBy(l => l));
            Commentry = new ObservableCollection<LinkItem>(currentLineLinks.Where(l => l.Conection_Type == "commentary" || l.Conection_Type == "targum"));
        }

        public class LinkItem
        {
            public double line_index_1 { get; set; }
            public string heRef_2 { get; set; }
            public string path_2 { get; set; }
            public double line_index_2 { get; set; }
            public string Conection_Type { get; set; }
        }
    }
}
