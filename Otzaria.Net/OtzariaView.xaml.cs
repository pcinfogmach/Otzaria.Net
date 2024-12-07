using Otzaria.Net.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Otzaria.Net
{
    /// <summary>
    /// Interaction logic for OtzariaView.xaml
    /// </summary>
    public partial class OtzariaView : UserControl
    {
        public OtzariaView()
        {
            InitializeComponent();
        }

        private void FileSystemItem_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is FileSystemItem fileSystemItem)
            {
                fileViewer.LoadFile(fileSystemItem.FullPath);
                viewModel.HasSearchResults = false;
                tabControl.SelectedIndex = 1;
            }
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down) { SearchResultsListBox.Focus(); e.Handled = true; }             
        }
    }
}
