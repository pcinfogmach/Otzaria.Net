using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OtzariaTestApp
{
    public class SQLiteFTS4Service 
    {
        public SQLiteFTS4Service()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var db = new SqliteDataBase())
            {
                db.ExecuteQuery(@"CREATE VIRTUAL TABLE IF NOT EXISTS documents USING fts4(
                    FilePath, Id, Tags, FullId, Level, Start, End,
                    notindexed=Level, notindexed=Start, notindexed=End
                );");
            }
        }

        public async Task IndexFolder(string folderPath)
        {
            await Task.Run(() =>
            {
                using (var db = new SqliteDataBase())
                {
                    // Parse folder and get the entries
                    var rootEntries = HtmlIndexParser.ParseFolder(folderPath);
                    var flattenedEntries = rootEntries.SelectMany(entry => entry.FlattenedEntries()).ToList();
                    int totalEntries = flattenedEntries.Count;
                    int processedEntries = 0;

                    new SQLiteCommand("begin", db.connection).ExecuteNonQuery();

                    try
                    {
                        // Use Parallel.ForEach to process entries in batches
                        Parallel.ForEach(flattenedEntries, entry =>
                        {
                            int currentEntryNumber = Interlocked.Increment(ref processedEntries);

                            // Perform the indexing
                            IndexEntry(entry, db);

                            // Check if a commit is needed
                            if (currentEntryNumber % 1000 == 0)
                            {
                                lock (db.connection) // Ensure thread safety for transaction commands
                                {
                                    using (var commitCommand = db.connection.CreateCommand())
                                    {
                                        commitCommand.CommandText = "COMMIT";
                                        commitCommand.ExecuteNonQuery();

                                        commitCommand.CommandText = "BEGIN";
                                        commitCommand.ExecuteNonQuery();
                                    }
                                }
                            }

                            // Log the progress
                            Console.WriteLine($"Processing {currentEntryNumber}/{totalEntries}: Entry ID {entry.Id}");
                        });

                        // Final commit after all entries are processed
                        lock (db.connection)
                        {
                            using (var finalCommitCommand = db.connection.CreateCommand())
                            {
                                finalCommitCommand.CommandText = "COMMIT";
                                finalCommitCommand.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rollback in case of error
                        lock (db.connection)
                        {
                            using (var rollbackCommand = db.connection.CreateCommand())
                            {
                                rollbackCommand.CommandText = "ROLLBACK";
                                rollbackCommand.ExecuteNonQuery();
                            }
                        }
                        Console.WriteLine($"An error occurred: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        private void IndexEntry(IndexEntry entry, SqliteDataBase db)
        {
            // Check if an entry with the same FullId exists
            var checkCommand = db.connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(1) FROM documents WHERE FullId = $FullId;";
            checkCommand.Parameters.AddWithValue("$FullId", entry.FullId ?? string.Empty);

            var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

            if (!exists)
            {
        //        // Update existing entry
        //        var updateCommand = db.connection.CreateCommand();
        //        updateCommand.CommandText = @"
        //UPDATE documents 
        //SET FilePath = $FilePath, 
        //    Id = $Id, 
        //    Tags = $Tags, 
        //    Level = $Level, 
        //    Start = $Start, 
        //    End = $End 
        //WHERE FullId = $FullId;";
        //        updateCommand.Parameters.AddWithValue("$FilePath", entry.FilePath ?? string.Empty);
        //        updateCommand.Parameters.AddWithValue("$FullId", entry.FullId ?? string.Empty);
        //        updateCommand.Parameters.AddWithValue("$Id", entry.Id ?? string.Empty);
        //        updateCommand.Parameters.AddWithValue("$Tags", entry.Tags ?? string.Empty);
        //        updateCommand.Parameters.AddWithValue("$Level", entry.Level);
        //        updateCommand.Parameters.AddWithValue("$Start", entry.Start);
        //        updateCommand.Parameters.AddWithValue("$End", entry.End);

        //        updateCommand.ExecuteNonQuery();
        //    }
        //    else
        //    {
                // Insert new entry
                var insertCommand = db.connection.CreateCommand();
                insertCommand.CommandText = @"
        INSERT INTO documents (FilePath, FullId, Id, Tags, Level, Start, End) 
        VALUES ($FilePath, $FullId, $Id, $Tags, $Level, $Start, $End);";
                insertCommand.Parameters.AddWithValue("$FilePath", entry.FilePath ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$FullId", entry.FullId ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$Id", entry.Id ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$Tags", entry.Tags ?? string.Empty);
                insertCommand.Parameters.AddWithValue("$Level", entry.Level);
                insertCommand.Parameters.AddWithValue("$Start", entry.Start);
                insertCommand.Parameters.AddWithValue("$End", entry.End);

                insertCommand.ExecuteNonQuery();
            }
        }




        //public void PrintAllIndexEntries()
        //{
        //    var command = connection.CreateCommand();
        //    command.CommandText = "SELECT * FROM documents;";
        //    using (var reader = command.ExecuteReader())
        //    {
        //        while (reader.Read())
        //        {
        //            Console.WriteLine($"Id: {reader["Id"]}, FilePath: {reader["FilePath"]}, FullId: {reader["FullId"]}");
        //        }
        //    }

        //}

        public List<SearchResult> Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                throw new ArgumentNullException(nameof(searchText), "Search text cannot be null or empty.");

            using (var db = new SqliteDataBase())
            {
                    // Clean the search text to prevent SQL injection or invalid characters
                    searchText = Regex.Replace(searchText, @"[^\w\s]+", "");

                    var command = db.connection.CreateCommand();
                    command.CommandText = @"
        SELECT FilePath, FullId, Id, Tags, Level, Start, End 
        FROM documents 
        WHERE documents MATCH $SearchText;";
                    command.Parameters.AddWithValue("$SearchText", searchText);

                    var results = new List<SearchResult>();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new SearchResult
                            {
                                FilePath = reader["FilePath"].ToString(),
                                Id = reader["Id"].ToString(),
                                Tags = reader["Tags"].ToString(),
                                Level = Convert.ToInt32(reader["Level"]),
                                Start = Convert.ToInt32(reader["Start"]),
                                End = Convert.ToInt32(reader["End"])
                            });
                        }
                    }

                    return results.OrderBy(r => r.Level)
                          .ThenBy(r => r.Id.Length)
                          .ThenBy(r => r.Id)
                          .ToList(); ;
            }
        }

    }
}
