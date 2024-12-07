using Otzaria.Net.Helpers;
using Otzaria.Net.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static Lucene.Net.Queries.Function.ValueSources.MultiFunction;

namespace Otzaria.Net
{
    internal class OtzariaViewModel : ViewModelBase
    {
        private string _searchTerm;
        private ObservableCollection<FileSystemItem> _searchResults = new ObservableCollection<FileSystemItem>();
        private ObservableCollection<FileSystemItem> _chapterResults = new ObservableCollection<FileSystemItem>();
        private bool _hasSearchResults;
        private bool _hasChapterResults;
        private FolderSystemItem _otzariaFileSystem = new FolderSystemItem("C:\\אוצריא\\אוצריא");

        public FolderSystemItem OtzariaFileSystem { get => _otzariaFileSystem; set => SetField(ref _otzariaFileSystem, value);}       

        public ObservableCollection<FileSystemItem> SearchResults
        {
            get => _searchResults;
            set {  if (SetField(ref _searchResults, value)) HasSearchResults = value.Count > 0;  }
        }

        public bool HasSearchResults
        {
            get => _hasSearchResults;
            set => SetField(ref _hasSearchResults, value);
        }

        public ObservableCollection<FileSystemItem> ChapterResults
        {
            get => _chapterResults;
            set { if (SetField(ref _chapterResults, value)) HasChapterResults = value.Count > 0; }
        }

        public bool HasChapterResults
        {
            get => _hasChapterResults;
            set => SetField(ref _hasChapterResults, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetField(ref _searchTerm, value) && !string.IsNullOrWhiteSpace(value) && _otzariaFileSystem != null)
                    Search(value);                      
                else
                    SearchResults = new ObservableCollection<FileSystemItem>();
            }
        }

        void Search(string searchTerm)
        {
            SearchResults = new ObservableCollection<FileSystemItem>(_otzariaFileSystem.Search(searchTerm));
        }

        public HtmlFileSystemItem SelectedFileTreeItem
        {
            set
            {
                if (value is HtmlFileSystemItem htmlFileSystemItem)
                {
                    //ChapterResults = htmlFileSystemItem.GetChapterHeaders(SearchTerm);
                }
            }
        }
    }
}
