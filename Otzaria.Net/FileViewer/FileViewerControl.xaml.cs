using FileSystemBrowser;
using Otzaria.Net;
using System.IO;
using System.Windows.Controls;
using static Otzaria.Net.FileViewer.LinksViewModel;

namespace FileViewer
{
    /// <summary>
    /// Interaction logic for FileViewerControl.xaml
    /// </summary>
    public partial class FileViewerControl : UserControl
    {
        OtzariaView _otzariaView;
        public FileViewerControl(FileSystemItem fileSystemItem, OtzariaView otzariaView)
        {
            _otzariaView = otzariaView;
            InitializeComponent();
            LoadItem(fileSystemItem);
        }

        async void LoadItem(FileSystemItem fileSystemItem)
        {
            string extension = Path.GetExtension(fileSystemItem.Path).ToLower();

            FileSystemItem rootItem = new FileSystemItem(fileSystemItem.Path, fileSystemItem.Path, true, 0);
            
            if (rootItem == null) return;

            string content = await FileSystemItemHelper.LoadFileContent(fileSystemItem.Path, rootItem, false, false);

            FsChapterViewer.RootItem = rootItem;

            int lineIndex = fileSystemItem.IsFile ? fileSystemItem.Index : 1;

            fileView.NavigateToContent(fileSystemItem, content, lineIndex);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is LinkItem linkItem)
            {
                _otzariaView.OpenLink(linkItem.path_2, (int)linkItem.line_index_2 - 2);
                listBox.SelectedIndex = -1;
            }               
        }
    }
}
