using Fastenshtein;
using FileSystemBrowser;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileSystemBrowser
{
    public class FileSystemLocater
    {
        FileSystemItem _root;
        public FileSystemLocater(FileSystemItem root)
        {
            _root = root;
        }

        public FileSystemItem LevinshtienSearch(string path)
        {
            var children = _root.EnumerateChildrenRecursive();

            // Create a Levenshtein distance instance for the input path
            var distanceCalculator = new Levenshtein(path);

            // Find the child with the smallest distance
            return children
                .OrderBy(child => distanceCalculator.DistanceFrom(child.Name))
                .FirstOrDefault();
        }

        public FileSystemItem WordBasedSearch(string path)
        {
            var searchWords = Regex.Split(path, @"\W+");

            FileSystemItem bestMatch = null;
            int maxMatchCount = 0;

            foreach (var child in _root.EnumerateChildrenRecursive())
            {
                int matchCount = 0;

                foreach (var word in searchWords)
                {
                    if (child.Name.Contains(word))
                    {
                        matchCount++;
                    }
                }

                // If this child has more matches than the previous best, update the best match
                if (matchCount > maxMatchCount)
                {
                    maxMatchCount = matchCount;
                    bestMatch = child;
                }
            }

            return bestMatch;
        }
    }
}
