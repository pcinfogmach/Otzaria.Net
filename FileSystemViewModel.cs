using FileSystemBrowser.Helpers;
using FileSystemBrowser.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Linq;
using System;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FileSystemBrowser.Browser
{

    public class FileSystemViewModel : ViewModelBase
    {
        private string _rootDirectory;
        private string _currentDirectory;
        private string _searchTerm;
        private CancellationTokenSource _cancellationTokenSource;
        private ObservableCollection<FileSystemItem> _items;

        public string CurrentDirectory { get => _currentDirectory; set => SetProperty(ref _currentDirectory, value); }
        public string SearchTerm { get => _searchTerm; set { SetProperty(ref _searchTerm, value); Search(value); } }
        public ObservableCollection<FileSystemItem> Items { get => _items; set => SetProperty(ref _items, value); }


        public FileSystemViewModel()
        {
            _rootDirectory = @"C:\אוצריא\אוצריא"; // Set your desired home directory
            _currentDirectory = _rootDirectory;
            LoadItems(_currentDirectory);

            GoBackCommand = new RelayCommand(GoBack, CanGoBack);
            GoHomeCommand = new RelayCommand(GoHome);
            NavigateToItemCommand = new RelayCommand<FileSystemItem>(NavigateTo);
        }

        public ICommand GoBackCommand { get; }
        public ICommand GoHomeCommand { get; }
        public ICommand NavigateToItemCommand { get; }

        private void GoBack()
        {
            var parentDirectory = Directory.GetParent(CurrentDirectory)?.FullName;
            if (parentDirectory != null)
            {
                CurrentDirectory = parentDirectory;
                LoadItems(CurrentDirectory);
            }
        }

        private void GoHome()
        {
            CurrentDirectory = _rootDirectory;
            LoadItems(CurrentDirectory);
        }

        public void NavigateTo(FileSystemItem item)
        {
            if (item.IsDirectory)
            {
                CurrentDirectory = item.FilePath;
                LoadItems(CurrentDirectory);
            }
        }

        private void LoadItems(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    // Define the custom order for directories
                    var directoryOrder = new[] { "תנך", "מדרש", "משנה", "תוספתא", "תלמוד בבלי", "תלמוד ירושלמי", "הלכה", "שות", "קבלה", "ספרי מוסר", "חסידות", "מחשבת ישראל", "סדר התפילה", "ספרות עזר", "אודות התוכנה" ,
                    "תורה", "נביאים", "כתובים", "תרגומים", "ראשונים", "אחרונים"};

                    // Define the custom order for file names (Hebrew book names)
                    var fileOrder = new[] { "בראשית", "שמות", "ויקרא", "מדבר", "דברים", };

                    // Order directories based on the custom order
                    var directories = Directory.GetDirectories(path)
                        .Select(d => new FileSystemItem(_rootDirectory, d, true))
                        .OrderBy(d => Array.IndexOf(directoryOrder, d.Name));

                    // Order files based on names containing the Hebrew book names
                    var files = Directory.GetFiles(path)
                        .Select(f => new FileSystemItem(_rootDirectory, f, false))
                        .OrderBy(f => Array.FindIndex(fileOrder, book => f.Name.Contains(book)));

                    // Combine and set the items collection
                    Items = new ObservableCollection<FileSystemItem>(directories.Concat(files));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void Search(string searchTerm)
        {
            _cancellationTokenSource?.Cancel(); // Cancel previous search if running
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                Items = await Task.Run(() =>
                {
                    string[] searchTerms = searchTerm.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (searchTerms.Length == 0 || token.IsCancellationRequested)
                        return new ObservableCollection<FileSystemItem>();

                    var files = new List<FileSystemItem>();
                    foreach (var path in Directory.GetFiles(_rootDirectory, "*", SearchOption.AllDirectories))
                    {
                        if (token.IsCancellationRequested)
                            return new ObservableCollection<FileSystemItem>();

                        bool containsAllTerms = searchTerms.All(term => path.Contains(term));
                        if (containsAllTerms)
                            files.Add(new FileSystemItem(_rootDirectory, path, false));
                    }

                    return new ObservableCollection<FileSystemItem>(files);
                }, token);
            }
            catch (OperationCanceledException)
            {
                // Search was canceled; no action needed
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private bool CanGoBack()
        {
            return !string.IsNullOrEmpty(CurrentDirectory) && CurrentDirectory != _rootDirectory;
        }
    }
}

