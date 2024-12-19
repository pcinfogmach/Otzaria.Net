using MyModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer
{
    public class FileViewerViewModel: ViewModelBase
    {
        int _fontSize = 100;
        string _fontFamily = "Arial";
        bool _showFonts;
        ObservableCollection<FontFamily> _fontList;
        
        public string FontFamily { get => _fontFamily; set { SetProperty(ref _fontFamily, value); ShowFonts = false; } }
        public int FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }
        public bool ShowFonts { get => _showFonts; set => SetProperty(ref _showFonts, value); }
        public ObservableCollection<FontFamily> FontList { get { if (_fontList == null) PopulateFontList(); return _fontList; } set => SetProperty(ref _fontList, value);  }

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
    }
}
