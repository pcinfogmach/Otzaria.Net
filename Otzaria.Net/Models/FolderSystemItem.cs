using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otzaria.Net.Models
{
    public class FolderSystemItem : FileSystemItem
    {
        public ObservableCollection<FileSystemItem> Children { get; } = new ObservableCollection<FileSystemItem>();

        public FolderSystemItem(string path, FolderSystemItem parent = null) : base(path, parent)
        {
            LoadChildren();
        }

        public void LoadChildren()
        {
            foreach (var dir in Directory.GetDirectories(FullPath)) try { Children.Add(new FolderSystemItem(dir, this)); } catch { }
            foreach (var file in Directory.GetFiles(FullPath))
            {
                try
                {
                    if (file.ToLower().EndsWith(".pdf"))
                        Children.Add(new PdfFileSystemItem(file, this));
                    else if (file.ToLower().EndsWith(".html") || file.ToLower().EndsWith(".txt"))
                        Children.Add(new HtmlFileSystemItem(file, this));
                    else
                        Children.Add(new FileSystemItem(file, this));
                }
                catch { }
            }
        }

        public IEnumerable<FileSystemItem> Search(string searchTerm)
        {
            var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Perform the search and calculate proximity scores
            var resultsWithScores = SearchInTree(this, terms)
                .Select(item => new
                {
                    Item = item,
                    Score = CalculateProximityScore(item.FullPath, terms)
                })
                .Where(result => result.Score >= 0); // Exclude items without all terms

            // Sort results by proximity score (lower is better)
            return resultsWithScores
                .OrderBy(result => result.Score)
                .Select(result => result.Item);
        }

        private static IEnumerable<FileSystemItem> SearchInTree(FolderSystemItem folder, string[] terms)
        {
            if (folder == null) yield break;

            foreach (var child in folder.Children)
            {
                if (!(child is FolderSystemItem) && Matches(child.FullPath, terms))
                    yield return child;

                if (child is FolderSystemItem subfolder)
                    foreach (var match in SearchInTree(subfolder, terms))
                        yield return match;
            }
        }

        private static int CalculateProximityScore(string fullPath, string[] terms)
        {
            // Split the path into words
            var words = fullPath.Split(new[] { '\\', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Find indices of terms in the split path
            var indices = terms
                .Select(term => words
                    .Select((word, index) => (word, index))
                    .Where(pair => pair.word.Contains(term))
                    .Select(pair => pair.index)
                    .ToList())
                .ToList();

            // If any term is not found, return -1 (invalid score)
            if (indices.Any(list => list.Count == 0))
                return -1;

            // Calculate the minimum proximity (difference between max and min indices)
            var minProximity = indices.SelectMany(i => i).Max() - indices.SelectMany(i => i).Min();

            return minProximity;
        }



        private static bool Matches(string path, string[] terms)
        {
            foreach (var term in terms) if (!path.Contains(term)) return false;
            return true;
        }
    }
}
