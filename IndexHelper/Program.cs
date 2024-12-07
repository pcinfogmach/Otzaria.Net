using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace IndexHelper
{
    internal class Program
    {
        static void Main()
        {
            string folderPath = @"C:\אוצריא\אוצריא";
            string outputPath = "IndexTestJson.json";

            HtmlIndexParser.ParseFolder(folderPath, outputPath);
        }

        public static class HtmlIndexParser
        {
            public static void ParseFolder(string folderPath, string outputPath)
            {
                List<IndexEntry> rootNodes = new List<IndexEntry>();

                // Get all .txt files and calculate total number of files
                string[] allFiles = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);
                int totalFiles = allFiles.Length;
                int fileNumber = 0;

                foreach (string filePath in allFiles)
                {
                    fileNumber++;
                    Console.WriteLine($"Parsing file {fileNumber}/{totalFiles}: {filePath}");
                    try
                    {
                        IndexEntry rootNode = ParseFile(filePath);
                        if (rootNode != null)
                        {
                            rootNodes.Add(rootNode);
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                }
            }

            

            public static IndexEntry ParseFile(string filePath)
            {
                string content = File.ReadAllText(filePath);
                string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                IndexEntry rootEntry = new IndexEntry(0, filePath, fileName, 1, 1);
                IndexEntry htmlParent = rootEntry;

                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i];
                    Match regexMatch = Regex.Match(line.ToLower().Trim(), @"^(\(([^( ]+)\)|<h([1-6])>)([^<(]+)");
                    if (regexMatch.Success)
                    {
                        int level = 1;
                        string levelString = regexMatch.Groups[3].ToString();
                        if (int.TryParse(levelString, out int L)) level = L;

                        string name = regexMatch.Groups[4].ToString();

                        if (!string.IsNullOrEmpty(regexMatch.Groups[2].ToString()))
                        {
                            level = 7;
                            name = regexMatch.Groups[2].ToString();
                        }

                        while (htmlParent.Parent is IndexEntry htmlGrandParent && level <= htmlParent.Level)
                            htmlParent = htmlGrandParent;

                        string id = $"{htmlParent.Id} {name}";

                        if (id == fileName) continue;

                        var newItem = new IndexEntry(level, filePath, id, i, i);

                        htmlParent.AddChild(newItem);
                        htmlParent = newItem;
                    }
                    else
                    {
                        htmlParent.IncrementEnd();
                    }
                }

                return rootEntry;
            }
        }

        public class IndexEntry
        {
            [JsonIgnore] public IndexEntry Parent { get; set; }
            [JsonIgnore] public int Level { get; set; }

            public string FilePath { get; set; }
            public string Id { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
            public List<IndexEntry> Children { get; set; } = new List<IndexEntry>();

            public IndexEntry(int level, string filePath, string id, int start, int end)
            {
                Level = level;
                FilePath = filePath;
                Id = id;
                Start = start;
                End = end;

                foreach (var child in Children) { child.Parent = this; }
            }

            public void AddChild(IndexEntry child)
            {
                Children.Add(child);
                child.Parent = this;
                IncrementEnd();
            }

            public void IncrementEnd()
            {
                End++;
                IncrementParentEnd();
            }

            public void IncrementParentEnd()
            {
                if (Parent != null) { Parent.End = this.End; Parent.IncrementParentEnd(); }
            }
        }
    }
}
