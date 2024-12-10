using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using MyHelpers;
using System.IO;

namespace FileSystemBrowser
{
    public class FileSystemViewModel : ViewModelBase
    {
        private Timer _unloadTimer;
        private const int UnloadTimeoutMilliseconds = 60000; // Adjust the timeout as needed       
        private FileSystemItem currentDirectory;

        private ObservableCollection<FileSystemItem> _items;
        private FileSystemItem _rootItem;
        private FileSystemItem _selectedItem;
        private string _previousSearchTerm = "";
        private string _searchTerm = "";
        bool _isSearching = false;

        public FileSystemItem RootItem { get => _rootItem; set { SetProperty(ref _rootItem, value);  Navigate(value); } }
        public FileSystemItem SelectedItem { get => _selectedItem;  set => SetProperty(ref _selectedItem, value);  }
        public ObservableCollection<FileSystemItem> Items { get => _items; set => SetProperty(ref _items, value); }
        public string SearchTerm { get => _searchTerm; set { SetProperty(ref _searchTerm, value); Search(); } }
        public bool IsSearching { get => _isSearching; set { SetProperty(ref _isSearching, value); } }
       

        public ICommand GoBackCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand NavigateToItemCommand { get; }
        public ICommand LoadHtmlTagsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public FileSystemViewModel()
        {
            GoBackCommand = new RelayCommand(GoBack, CanGoBack);
            GoHomeCommand = new RelayCommand(GoHome);
            NavigateToItemCommand = new RelayCommand<FileSystemItem>(NavigateTo);
            LoadHtmlTagsCommand = new RelayCommand<FileSystemItem>(LoadHtmlTags);
            SearchCommand = new RelayCommand(Search);
            ClearSearchCommand = new RelayCommand(() => SearchTerm = "");
        }

        async void Initialize(string root)
        {
            bool isDirectory = Directory.Exists(root); 
            string extension = Path.GetExtension(root);
            
            if (isDirectory)
                _rootItem = new FileSystemItem(root, root, isDirectory, -10);
            else if (extension.Contains(".txt") || extension.Contains(".html"))
                _rootItem = new HtmlFileSystemItem(root, root, isDirectory, 0);

            foreach (HtmlFileSystemItem child in _rootItem.Children)
                await child.LoadContent(_rootItem.Path, true, true);

            if (_rootItem is HtmlFileSystemItem htmlItem)
                await htmlItem.LoadContent(_rootItem.Path, true, true);
            else
                StartUnloadTimer();

            Navigate(_rootItem);
        }

        private void StartUnloadTimer()
        {
            _unloadTimer = new Timer(async state =>
            {
                await Task.Run(() => { Reset(_rootItem); }); // Unload children when timer elapses
            }, null, UnloadTimeoutMilliseconds, Timeout.Infinite);
        }

        public void Reset(FileSystemItem currentItem)
        {
            if (currentItem is HtmlFileSystemItem htmlItem)
                htmlItem.CancelLoadingTags();

            foreach (var child in currentItem.Children)
                Reset(child);
        }

        private async void GoBack()
        {
            if (currentDirectory.Parent != null) 
            {
                if (currentDirectory.Parent.IsDirectory)
                {
                    NavigateTo(currentDirectory.Parent);
                }
                else if (currentDirectory.Parent.Children.Count == 0)
                {
                    currentDirectory = SearchByPath(currentDirectory.Path, _rootItem);
                    if (currentDirectory is HtmlFileSystemItem htmlItem) 
                        await htmlItem.LoadContent(_rootItem.Path, true, true);
                    Items = currentDirectory.Children;
                }
                else
                {
                   Navigate(currentDirectory.Parent);
                }
            }
        }
       
        private void GoHome() => NavigateTo(_rootItem);

        public async void NavigateTo(FileSystemItem item)
        {
            if (item != null && item.IsDirectory)
            {
                var loadTasks = item.Children
                    .OfType<HtmlFileSystemItem>()
                    .Select(htmlItem => htmlItem.LoadContent(_rootItem.Path, true, true));

                await Task.WhenAll(loadTasks);

                Navigate(item);
            }
            else
            {
                SelectedItem = item;
            }
        }

        void Navigate(FileSystemItem item)
        {
            currentDirectory = item;
            Items = item.Children;
        }

        public FileSystemItem SearchByPath(string filePath, FileSystemItem currentItem)
        {
            if (filePath == currentItem.Path)
            {
                return currentItem;
            }
                
            foreach (var child in currentItem.Children)
            {
                var result = SearchByPath(filePath, child);
                if (result != null)
                    return result;
            }

            return null;
        }


        public async void LoadHtmlTags(FileSystemItem item)
        {
            if (!(item is HtmlFileSystemItem htmlFile)) return;
            
            if (htmlFile.Children.Count == 0)
                await htmlFile.LoadContent(_rootItem.Path, true, true);
           
            Navigate(htmlFile);
        }

        private async void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Items = currentDirectory.Children;
                return;
            }

            IsSearching = true;
            _previousSearchTerm = SearchTerm;

            // Split search terms and prepare results
            var SearchTerms = SearchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var results = new List<(FileSystemItem Item, int OriginalIndex)>();

            // Perform the search and capture the original index
            int index = 0;
            SearchFileSystem(_rootItem, SearchTerms, results, ref index);

            // Load tags asynchronously for specific HTML items
            if (SearchTerm.Length > 3 && !_rootItem.Children.Any(c => SearchTerms.All(term => c.Name.Contains(term))))
            {
                var htmlItems = results
                    .Select(r => r.Item)
                    .OfType<HtmlFileSystemItem>()
                    .Where(item => item.Parent.IsDirectory == true
                                   && !item.IsTagsLoaded
                                   && item.Children.Count == 0);

                foreach (var item in htmlItems)
                {
                    await item.LoadContent(_rootItem.Path, true, true);
                }
            }

            // Order results by level and original index
            var orderedResults = results
                .OrderBy(r => r.Item.Name.Length)
                .ThenBy(r => r.Item.Level)
                .ThenBy(r => r.OriginalIndex)
                .Select(r => r.Item);

            // Update the observable collection
            Items = new ObservableCollection<FileSystemItem>(orderedResults);

            IsSearching = false;
        }

        private void SearchFileSystem(FileSystemItem item, string[] terms, List<(FileSystemItem Item, int OriginalIndex)> results,ref int index)
        {
            string extendedName = item.Path + " " + item.Name;
            // Split item.Name into words using delimiters
            var nameWords = extendedName.Split(new[] { ' ', '_', '-', ',', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            // Check if all terms are fully contained in the name words
            if (terms.All(term => nameWords.Contains(term, StringComparer.OrdinalIgnoreCase)))
            {
                results.Add((item, index++));
            }

            // Recursively search through child items
            foreach (var child in item.Children)
            {
                SearchFileSystem(child, terms, results, ref index);
            }
        }


        private bool CanGoBack() => currentDirectory?.Parent != null;
    }
}

