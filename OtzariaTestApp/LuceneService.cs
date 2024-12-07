using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace OtzariaTestApp
{
    public class LuceneService
    {
        string _indexPath;
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        public LuceneService(string indexPath)
        {
            if (!System.IO.Directory.Exists(indexPath)) { System.IO.Directory.CreateDirectory(indexPath); }
            _indexPath = indexPath;
        }

        public void IndexFolder(string folderPath)
        {
            var _directory = FSDirectory.Open(_indexPath);
            var _analyzer = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            var _indexWriter = new IndexWriter(_directory, indexConfig);

            var rootEntries = HtmlIndexParser.ParseFolder(folderPath);
            foreach (var rootEntry in rootEntries)
            {
                var allEntries = rootEntry.FlattenedEntries(); // Ensure the collection is materialized
                int count = allEntries.Count;
                int index = 0;

                // Use a thread-safe object for progress reporting
                object lockObject = new object();

                Parallel.ForEach(allEntries, entry =>
                {
                    int currentIndex;
                    lock (lockObject)
                    {
                        index++;
                        currentIndex = index;
                    }

                    Console.WriteLine($"Root: {rootEntries.IndexOf(rootEntry)}/{rootEntries.Count} Entry: {currentIndex}/{count}");
                    IndexEntry(entry, _indexWriter);
                });
            }

            _indexWriter.Flush(triggerMerge: true, applyAllDeletes: true);
            _indexWriter.Dispose();
            _analyzer.Dispose();
            _directory.Dispose();
        }


        private void IndexEntry(IndexEntry entry, IndexWriter indexWriter)
        {
            var doc = new Document
            {
                new StringField("FilePath", entry.FilePath, Field.Store.YES),
                new StringField("Id", entry.Id, Field.Store.YES),
                new StringField("Tags", entry.Tags, Field.Store.YES),
                new TextField("FullId", entry.FullId, Field.Store.YES),
                new Int32Field("Level", entry.Level, Field.Store.YES),
                new Int32Field("Start", entry.Start, Field.Store.YES),
                new Int32Field("End", entry.End, Field.Store.YES),
            };
            indexWriter.AddDocument(doc);
        }



        public void PrintAllIndexEntries()
        {
            var _directory = FSDirectory.Open(_indexPath);
            var reader = DirectoryReader.Open(_directory);
            try
            {
                Console.WriteLine($"Total Documents in Index: {reader.NumDocs}");

                for (int i = 0; i < reader.MaxDoc; i++)
                {
                    var doc = reader.Document(i);
                    Console.WriteLine($"Document {i}:");
                    foreach (var field in doc.GetFields("FullId"))
                    {
                        Console.WriteLine($"  {field.Name}: {doc.Get(field.Name)}");
                    }
                    Console.WriteLine("------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading index: {ex.Message}");
            }
            finally
            {
                reader.Dispose();
                _directory.Dispose();
            }
        }


        public List<SearchResult> Search(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) throw new ArgumentNullException("Invalid Text Input");
            searchText = Regex.Replace(searchText, @"[^\w\s]+", "");
            var _directory = FSDirectory.Open(_indexPath);
            var _analyzer = new StandardAnalyzer(AppLuceneVersion);
            var searcher = new IndexSearcher(DirectoryReader.Open(_directory));

            var searchTerms = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var booleanQuery = new BooleanQuery();
            foreach (var term in searchTerms)
                booleanQuery.Add(new TermQuery(new Term("FullId", term)), Occur.MUST);

            var hits = searcher.Search(booleanQuery, searchTerms.Length * 10000).ScoreDocs;

            var results = new List<SearchResult>();
            foreach (var hit in hits)
            {
                var doc = searcher.Doc(hit.Doc);
                
                var result = new SearchResult();
                result.Id = doc.Get("Id");
                result.FilePath = doc.Get("FilePath");
                result.Tags = doc.Get("Tags");
                try
                {
                    result.Level = int.Parse(doc.Get("Level"));
                    result.Start = int.Parse(doc.Get("Start"));
                    result.End = int.Parse(doc.Get("End"));
                }
                catch { }

                results.Add(result);
            }

            _directory.Dispose();
            _analyzer.Dispose();

            return results.OrderBy(r => r.Level)                         
                          .ThenBy(r => r.Id.Length)
                          .ThenBy(r => r.Id)
                          .ToList();
        }

    }

    public class SearchResult
    {
        public string FullId { get; set; }
        public string Id { get; set; }
        public string Tags { get; set; }
        public string FilePath { get; set; }
        public int Level { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public override string ToString()
        {
            return Id;
        }
    }
}
