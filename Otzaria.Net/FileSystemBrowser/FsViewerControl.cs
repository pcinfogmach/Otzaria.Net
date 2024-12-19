using FileSystemBrowser.Browser;
using MyHelpers;
using MyModels;
using MyXamlHelpers;
using Otzaria.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileSystemBrowser
{
    public class FsViewerControl : Control
    {
        #region  DependencyProperties
        public static readonly DependencyProperty RootItemProperty = DependencyProperty.Register("RootItem", typeof(FileSystemItem), typeof(FsViewerControl),
             new PropertyMetadata(null, OnRootItemChanged));
        public FileSystemItem RootItem { get => (FileSystemItem)GetValue(RootItemProperty); set => SetValue(RootItemProperty, value); }

        public static readonly DependencyProperty RootDirectoryProperty = DependencyProperty.Register("RootDirectory", typeof(string), typeof(FsViewerControl),
            new PropertyMetadata(null, OnRootDirectoryChanged));
        public string RootDirectory { get => (string)GetValue(RootDirectoryProperty); set => SetValue(RootDirectoryProperty, value); }

        public static readonly DependencyProperty CurrentItemProperty = DependencyProperty.Register("CurrentItem", typeof(FileSystemItem), typeof(FsViewerControl),
            new PropertyMetadata(null, OnCurrentItemChanged));
        public FileSystemItem CurrentItem { get => (FileSystemItem)GetValue(CurrentItemProperty); set => SetValue(CurrentItemProperty, value); }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(ObservableCollection<FileSystemItem>), typeof(FsViewerControl));
        public ObservableCollection<FileSystemItem> Items { get => (ObservableCollection<FileSystemItem>)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(FileSystemItem), typeof(FsViewerControl));
        public FileSystemItem SelectedItem { get => (FileSystemItem)GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }

        public static readonly DependencyProperty SearchTermProperty = DependencyProperty.Register("SearchTerm", typeof(string), typeof(FsViewerControl),
             new PropertyMetadata(string.Empty, OnSearchTermChanged));
        public string SearchTerm { get => (string)GetValue(SearchTermProperty); set => SetValue(SearchTermProperty, value); }

        public static readonly DependencyProperty IsSearchingProperty = DependencyProperty.Register("IsSearching", typeof(bool), typeof(FsViewerControl));
        public bool IsSearching { get => (bool)GetValue(IsSearchingProperty); set => SetValue(IsSearchingProperty, value); }

        private static void OnRootDirectoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
           => ((FsViewerControl)d).InitializeRoot((string)e.NewValue);

        private static void OnRootItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
           => ((FsViewerControl)d).CurrentItem = ((FileSystemItem)e.NewValue);

        private static void OnSearchTermChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
           => ((FsViewerControl)d).Search((string)e.NewValue);

        private async static void OnCurrentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FsViewerControl)d;
            var fileSystemItem = (FileSystemItem)e.NewValue;

            if (fileSystemItem.IsFile == false)
                await FileSystemItemHelper.LoadFilesContentHeaders(control.RootItem.Path, fileSystemItem.Children);
            else if (fileSystemItem.Parent?.IsFile == false)
                await FileSystemItemHelper.LoadFileContent(control.RootItem.Path, fileSystemItem, loadTagsOnly: true, useFullName: false);

            control.Items = fileSystemItem.Children.ToObservableCollection();
        }
        #endregion

        #region Commands
        public ICommand GoBackCommand => new RelayCommand(GoBack, CanGoBack);
        public ICommand GoHomeCommand => new RelayCommand(GoHome);
        public ICommand SelectItemCommand => new RelayCommand<FileSystemItem>(SelectItem);
        public ICommand NavigateCommand => new RelayCommand<FileSystemItem>(NavigateTo);
        public ICommand ClearSearchCommand => new RelayCommand(() => SearchTerm = "");
        #endregion

        private CancellationTokenSource _searchCancellationTokenSource;
        private string previousSearchTerm = string.Empty;

        public FsViewerControl()
        {
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Down && e.OriginalSource is TextBox)
                {
                    var listBox = MyVisualTreeHelper.FindChild<ListBox>(this);
                    if (listBox != null) listBox.Focus();
                }
            };
        }

        #region Methods
        public void InitializeRoot(string path)
        {
            bool isFile = File.Exists(path);
            string extension = Path.GetExtension(path);
            int level = isFile ? 0 : short.MinValue;

            RootItem = new FileSystemItem(path, path, isFile);

            CurrentItem = RootItem;
        }

        private void GoHome()
        {
            if (CurrentItem == RootItem)
                Items = RootItem.Children.ToObservableCollection();
            else // when search is in progress
                CurrentItem = RootItem;
        }

        private void GoBack()
        {
            if (Items.SequenceEqual(CurrentItem.Children)) 
                CurrentItem = CurrentItem.Parent; // when search is in progress
            else
                Items = CurrentItem.Children.ToObservableCollection();
        }


        private void NavigateTo(FileSystemItem item) => CurrentItem = item;

        private bool CanGoBack() => CurrentItem?.Parent != null || SearchTerm.Length > 0;

        public void SelectItem(FileSystemItem item)
        {
            if (item != null && !item.IsFile)
                CurrentItem = item;
            else
                SelectedItem = item;
        }

        #endregion

        #region Search
        private async void Search(string searchTerm)
        {
            searchTerm = FileSystemItemHelper.CleanNonWordChars(searchTerm);
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var cancelToken = _searchCancellationTokenSource.Token;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Items = new ObservableCollection<FileSystemItem>(CurrentItem.Children);
                return;
            }

            IsSearching = true;
            previousSearchTerm = searchTerm;

            // Split search terms and prepare results list
            var searchTerms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var results = await FileSystemSearcher.SearchByLevelOrder(RootItem, searchTerm);

            // Load header tags asynchronously for HTML files
            if (searchTerm.Length > 3 && !RootItem.Children.Any(c => searchTerms.All(term => c.Name.Contains(term))))
                await FileSystemItemHelper.LoadFilesContentHeaders(RootItem.Path, results);

            if (!cancelToken.IsCancellationRequested)
                Items = new ObservableCollection<FileSystemItem>( results);

            IsSearching = false;
        }
        #endregion
    }
}
