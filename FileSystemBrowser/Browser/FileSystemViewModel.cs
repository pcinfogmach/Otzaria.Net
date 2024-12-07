using FileSystemBrowser.Helpers;
using FileSystemBrowser.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;

namespace FileSystemBrowser.Browser
{
    public class FileSystemViewModel : ViewModelBase
    {
        private FileSystemItem currentDirectory;
        private string _searchTerm;
        private ObservableCollection<FileSystemItem> _items;
        private FileSystemItem _rootItem;
        private CancellationTokenSource _searchCancellationTokenSource;

        public string SearchTerm { get => _searchTerm; set { SetProperty(ref _searchTerm, value); Search(value); } }
        public ObservableCollection<FileSystemItem> Items { get => _items; set => SetProperty(ref _items, value); }

        public ICommand GoBackCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand NavigateToItemCommand { get; }
        public ICommand LoadChildrenCommand { get; }

        public FileSystemViewModel()
        {
            string rootDirectory = @"C:\אוצריא\אוצריא"; // Set your desired home directory

            // Initialize the root structure
            _rootItem = new FileSystemItem(rootDirectory, rootDirectory, true);
            currentDirectory = _rootItem;
            Items = _rootItem.Children;

            GoBackCommand = new RelayCommand(GoBack, CanGoBack);
            GoHomeCommand = new RelayCommand(GoHome);
            NavigateToItemCommand = new RelayCommand<FileSystemItem>(NavigateTo);
            LoadChildrenCommand = new RelayCommand<FileSystemItem>(LoadChildren);
        }

        private void GoBack()
        {
            if (currentDirectory != _rootItem)
            {
                currentDirectory = currentDirectory.Parent;
                Items = currentDirectory.Children;
            }
        }

        private void GoHome()
        {
            currentDirectory = _rootItem;
            Items =_rootItem.Children;
        }

        public async void NavigateTo(FileSystemItem item)
        {
            if (item.IsDirectory)
            {
                currentDirectory = item;

                var loadTasks = item.Children
                    .OfType<HtmlFileSystemItem>()
                    .Select(htmlItem => htmlItem.LoadTags(_rootItem.Path));

                await Task.WhenAll(loadTasks);

                Items = item.Children;
            }
        }

        public void LoadChildren(FileSystemItem item)
        {
            if (!(item is HtmlFileSystemItem)) return;
            Items = item.Children;
            currentDirectory = item;
        }

        private async void Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            // Split search terms and prepare results
            var searchTerms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var results = new List<(FileSystemItem Item, int OriginalIndex)>();

            // Perform the search and capture the original index
            int index = 0;
            SearchFileSystem(_rootItem, searchTerms, results, ref index);

            // Order results by level and original index
            var orderedResults = results
                .OrderBy(r => r.Item.Level)
                .ThenBy(r => r.OriginalIndex)
                .Select(r => r.Item);

            // Update the observable collection
            Items = new ObservableCollection<FileSystemItem>(orderedResults);

            // Load tags asynchronously for specific HTML items
            if (searchTerm.Length > 3)
            {
                var htmlItems = orderedResults
                    .OfType<HtmlFileSystemItem>()
                    .Where(item => item.Parent?.IsDirectory == true
                                   && !item.IsTagsLoaded
                                   && item.Children.Count == 0);

                foreach (var item in htmlItems)
                {
                    await item.LoadTags(_rootItem.Path);
                }
            }
        }

        private void SearchFileSystem(
            FileSystemItem item,
            string[] terms,
            List<(FileSystemItem Item, int OriginalIndex)> results,
            ref int index)
        {
            // Split item.Name into words using delimiters
            var nameWords = item.Name.Split(new[] { ' ', '_', '-', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

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


        private bool CanGoBack() => currentDirectory.Parent != null;
    }
}

