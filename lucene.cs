using System;

public class Class1
{
	public Class1()
	{

        //// Create an initial SpanNearQuery to enforce term order
        //var spanQueries = new List<SpanQuery>();
        //foreach (var term in searchTerms)
        //    spanQueries.Add(new SpanTermQuery(new Term("Id", term)));

        //var spanNearQuery = new SpanNearQuery(spanQueries.ToArray(), 10, true); // true for enforcing term order

        //// If no results, perform an unordered SpanNearQuery
        //if (hits.Length == 0)
        //{
        //    var unorderedSpanNearQuery = new SpanNearQuery(spanQueries.ToArray(), 10, false); // false for unordered
        //    hits = searcher.Search(unorderedSpanNearQuery, 5000).ScoreDocs;
        //}

 //       return results
 //.OrderBy(r => r.Id.Length)
 //.ThenBy(r =>
 //{
 //    var idWords = r.Id.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
 //    return CalculateWordLevenshteinDistance(idWords, searchTerms);
 //})
 //.ThenByDescending(r =>
 //{
 //    var idWords = r.Id.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

 //    return CalculateConsecutiveMatchScore(idWords, searchTerms);
 //})
 //.ThenBy(r => r.Id, StringComparer.OrdinalIgnoreCase) // Alphabetical fallback
 //.ToList();
 //   }

 //   // Method to calculate word-based Levenshtein distance
 //   int CalculateWordLevenshteinDistance(string[] idWords, string[] queryWords)
 //   {
 //       int[,] dp = new int[queryWords.Length + 1, idWords.Length + 1];

 //       // Initialize DP table
 //       for (int i = 0; i <= queryWords.Length; i++) dp[i, 0] = i;
 //       for (int j = 0; j <= idWords.Length; j++) dp[0, j] = j;

 //       // Fill DP table
 //       for (int i = 1; i <= queryWords.Length; i++)
 //       {
 //           for (int j = 1; j <= idWords.Length; j++)
 //           {
 //               if (queryWords[i - 1].Equals(idWords[j - 1], StringComparison.OrdinalIgnoreCase))
 //               {
 //                   dp[i, j] = dp[i - 1, j - 1]; // No cost for match
 //               }
 //               else
 //               {
 //                   dp[i, j] = Math.Min(
 //                       dp[i - 1, j - 1] + 1, // Substitution
 //                       Math.Min(
 //                           dp[i, j - 1] + 1, // Insertion
 //                           dp[i - 1, j] + 1  // Deletion
 //                       )
 //                   );
 //               }
 //           }
 //       }

 //       return dp[queryWords.Length, idWords.Length];
 //   }

 //   // Method to calculate consecutive match score
 //   int CalculateConsecutiveMatchScore(string[] idWords, string[] queryWords)
 //   {
 //       int score = 0;

 //       for (int i = 0; i < queryWords.Length - 1; i++)
 //       {
 //           for (int j = 0; j < idWords.Length - 1; j++)
 //           {
 //               if (idWords[j].Equals(queryWords[i], StringComparison.OrdinalIgnoreCase) &&
 //                   idWords[j + 1].Equals(queryWords[i + 1], StringComparison.OrdinalIgnoreCase))
 //               {
 //                   score++; // Increment score for each consecutive match
 //               }
 //           }
 //       }

 //       return score;
 //   }



 //   // Word-based Levenshtein distance algorithm
 //   private int GetWordBasedLevenshteinDistance(string source, string target)
 //   {
 //       // Split the source and target strings into words (split by whitespace)
 //       var sourceWords = source.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
 //       var targetWords = target.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

 //       var n = sourceWords.Length;
 //       var m = targetWords.Length;
 //       var d = new int[n + 1, m + 1];

 //       // Initialize the matrix
 //       for (var i = 0; i <= n; i++) d[i, 0] = i;
 //       for (var j = 0; j <= m; j++) d[0, j] = j;

 //       // Compute the word-based Levenshtein distance
 //       for (var i = 1; i <= n; i++)
 //       {
 //           for (var j = 1; j <= m; j++)
 //           {
 //               var cost = (sourceWords[i - 1] == targetWords[j - 1]) ? 0 : 1;
 //               d[i, j] = new[] {
 //                   d[i - 1, j] + 1,   // Deletion
 //                   d[i, j - 1] + 1,   // Insertion
 //                   d[i - 1, j - 1] + cost  // Substitution
 //               }.Min();
 //           }
 //       }

 //       return d[n, m];
 //   }
}
}
