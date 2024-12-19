using DocumentFormat.OpenXml.Spreadsheet;
using FileSystemBrowser;
using FileViewer;
using MyControls;
using MyHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Otzaria.Net
{
    /// <summary>
    /// Interaction logic for OtzariaView.xaml
    /// </summary>
    
    public partial class OtzariaView : UserControl, INotifyPropertyChanged
    {
        #region InterFace
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        private FileSystemItem _selectedItem;
        public FileSystemItem SelectedItem 
        { 
            get => _selectedItem;
            set { SetProperty(ref _selectedItem, value); OpenNewFile(value); }
        }

        public OtzariaView()
        {
            Globals.InitializeRoot("C:\\אוצריא\\אוצריא");           
            InitializeComponent();
            FSNavPanel.RootItem = Globals.RootItem;
        }

       

        public void OpenLink(string linkPath, int lineIndex)
        {
            if (string.IsNullOrEmpty(linkPath)) return;
            linkPath = linkPath.Replace("אוצריא\\", "");
            linkPath = Path.Combine(Globals.RootItem.Path, linkPath);
            if (!File.Exists(linkPath)) {linkPath = new FileSystemLocater(Globals.RootItem).WordBasedSearch(linkPath).Path; }
            FileSystemItem newItem = new FileSystemItem(linkPath, linkPath, true,index: lineIndex);
            OpenNewFile(newItem);
        }

        public void OpenNewFile(FileSystemItem item)
        {
            if (string.IsNullOrEmpty(item.Path) || !File.Exists(item.Path)) return;

            object viewer = null;
            var tabItem = new TabItem { Header = Path.GetFileNameWithoutExtension(item.Path), IsSelected = true };

            string extension = Path.GetExtension(item.Path).ToLower();

            if (extension == ".pdf")
            {
                var newItem = new WebView2Base { Source = new Uri(item.Path) };
                newItem.SetBinding(tabItem);
                viewer = newItem;
            }
            else if (extension == ".txt" || extension == ".html" || GenericHelpers.WordDocumentExtensions.Contains(extension))
            {
                var newItem = new FileViewerControl(item, this);
                newItem.fileView.SetBinding(tabItem);
                viewer = newItem;
            }

            if (viewer != null)
            {
                tabItem.Content = viewer;
                ViewerTabControl.Items.Add(tabItem);
                PagerTabControl.SelectedIndex = 1;
            }
        }


        private void PagerTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PagerTabControl.SelectedIndex == -1) PagerTabControl.SelectedIndex = 1;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.O)
                {
                    PagerTabControl.SelectedIndex = 0; e.Handled = true;
                }
                else if (e.Key == Key.W)
                {
                    ViewerTabControl.CloseTab((TabItem)ViewerTabControl.SelectedItem);
                    e.Handled = true;
                }
                else if (e.Key == Key.X)
                {
                    var tabs = new List<TabItem>(ViewerTabControl.Items.Cast<TabItem>());
                    foreach (TabItem tabItem in tabs)
                        ViewerTabControl.CloseTab(tabItem);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape && PagerTabControl.SelectedIndex == 0) 
            {
                PagerTabControl.SelectedIndex = 1; e.Handled = true;
            }
        }

        private void OpenFileToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (PagerTabControl.SelectedIndex <= 0) PagerTabControl.SelectedIndex = 1;
        }
    }
}
