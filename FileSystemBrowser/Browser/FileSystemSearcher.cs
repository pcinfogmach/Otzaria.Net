using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemBrowser
{
    public static class FileSystemSearcher
    {
        public static async Task<List<FileSystemItem>> SearchByLevelOrder(FileSystemItem fileSystemItem, string searchTerms)
        {
            return await Task.Run(async () =>
            {
                // Split and normalize the search terms by removing empty entries and converting to lowercase.
                var splitSearchTerms = searchTerms
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(term => term.ToLowerInvariant())
                    .ToArray();

                // Initialize a list to hold the items along with their corresponding scores.
                var results = new List<(FileSystemItem Item, int Score)>();

                // Start the recursive search with the root level item and the split search terms.
                await SearchRecursive(fileSystemItem, splitSearchTerms, results);

                // Order the results by the calculated score in descending order and return the top 100 items.
                return results
                    .OrderByDescending(result => result.Score)
                    .ThenBy(result => result.Item.Name) // Use a property to compare (Name, Id, etc.)
                    .Select(result => result.Item)
                    .Take(100)
                    .ToList();
            });
        }

        private static async Task SearchRecursive(FileSystemItem currentItem, string[] splitSearchTerms, List<(FileSystemItem Item, int Score)> results)
        {
            // Iterate over all the child items of the current item.
            foreach (var item in currentItem.Children)
            {
                // Split and normalize the item name by separating on common delimiters, converting to lowercase.
                var splitName = item.ExtendedName()
                    .Split(new[] { ' ', '_', '-', ',', '/', '\\', '.', ':' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(name => name.ToLowerInvariant())
                    .ToArray();

                // Calculate the score for the current item based on the search terms and the normalized name.
                var score = CalculateScore(item, splitSearchTerms, splitName);

                // If the score is above a threshold, add the item and its score to the results list.
                if (score > -1000)
                    results.Add((item, score));

                // Recursively search through the child items.
                await SearchRecursive(item, splitSearchTerms, results);
            }
        }



        private static int CalculateScore(FileSystemItem item, string[] searchTerms, string[] splitName)
        {
            int score = 0;

            // Check if all search terms are found in the item name.
            if (!searchTerms.All(term => splitName.Contains(term, StringComparer.OrdinalIgnoreCase)))
            {
                score -= 1000;
                return score;
            }

            score -= item.Level;

            if (item.Level == 0)
                score += 10;

            // Full exact match of the entire search phrase.
            if (item.ExtendedName().Equals(string.Join(" ", searchTerms), StringComparison.OrdinalIgnoreCase))
                score += 3;

            // Name ends with the last search term.
            if (!splitName.Last().Equals(searchTerms.Last(), StringComparison.OrdinalIgnoreCase) == true)
                score -= 10;

            // Name starts with the first search term.
            if (item.Name.StartsWith(searchTerms.First(), StringComparison.OrdinalIgnoreCase) == true)
                score += 4;

            return score;
        }
    }
}
