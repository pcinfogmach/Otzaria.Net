using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OtzariaTestApp
{
    public static class HtmlIndexParser
    {
        static Regex nonWordCharsRegex = new Regex(@"[^\w\s]+", RegexOptions.Compiled);
        public static List<IndexEntry> ParseFolder(string folderPath)
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

            return rootNodes;
        }



        public static IndexEntry ParseFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            string[] lines = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            fileName = nonWordCharsRegex.Replace(fileName, "");

            IndexEntry rootEntry = new IndexEntry(0, filePath, fileName, 1, 1);
            IndexEntry htmlParent = rootEntry;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                Match regexMatch = Regex.Match(line.ToLower().Trim(), @"^(\((?<verse>[^( ]+)\)|<h(?<level>[1-6])>)(?<content>[^<\n]+)");
                if (regexMatch.Success)
                {
                    int level = 1;
                    string levelString = regexMatch.Groups["level"].Value;
                    if (int.TryParse(levelString, out int L)) level = L;
                    if (line.Contains("פתיחה") || line.Contains("הקדמה"))
                    {
                        level = 4;
                    }

                    string name = regexMatch.Groups["content"].Value;
                    if (!string.IsNullOrEmpty(regexMatch.Groups["verse"].Value))
                    {
                        level = 7;
                        name = regexMatch.Groups["verse"].Value;
                    }
                    name = nonWordCharsRegex.Replace(name, "");

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
            NormalizeLevel7(rootEntry);
            return rootEntry;
        }

        static void NormalizeLevel7(IndexEntry parent)
        {
            foreach (var child in parent.Children)
            {
                if (child.Level == 7) {child.Level = parent.Level + 1;}
            }

            foreach (var child in parent.Children)
                NormalizeLevel7(child);
        }
    }

    public class IndexEntry
    {
        public IndexEntry Parent { get; set; }
        public int Level { get; set; }

        public string FilePath { get; set; }
        public string Id { get; set; }
        public string FullId { get; set; }
        public string Tags { get; set; }
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

            Tags = Path.GetDirectoryName(filePath).Replace("C:\\", "").Replace("אוצריא\\", "").Replace("\\", " \\ ").Trim();
            FullId =  Tags + " " + id;
            Tags += " \\ " + Path.GetExtension(filePath);

            ////json parse child parent association
            //foreach (var child in Children) { child.Parent = this; }
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

        public List<IndexEntry> FlattenedEntries()
        {
            var entries = new List<IndexEntry>();
            TraverseEntries(this, ref entries);
            return entries;
        }

        private void TraverseEntries(IndexEntry entry, ref List<IndexEntry> entries)
        {
            entries.Add(entry);
            foreach (var child in entry.Children)
            {
                TraverseEntries(child, ref entries);
            }
        }
    }
}
