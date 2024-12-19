using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace FileSystemBrowser
{
    public static class FileSystemItemHelper
    {
        // Define the custom order for directories
        public static string[] directoryOrder = new string[] { "תנך", "מדרש", "משנה", "תוספתא", "תלמוד בבלי", "תלמוד ירושלמי", "הלכה", "שות", "קבלה", "ספרי מוסר", "חסידות", "מחשבת ישראל", "סדר התפילה", "ספרות עזר", "אודות התוכנה" ,
                    "תורה", "נביאים", "כתובים", "תרגומים", "ראשונים", "אחרונים"};

        // Define the custom order for file names (Hebrew book names)
        public static string[] fileOrder = new string[]
         {
            // תורה
            "בראשית", "שמות", "ויקרא", "במדבר", "דברים",

            // נביאים ראשונים
            "יהושע", "שופטים", "שמואל א", "שמואל ב", "מלכים א", "מלכים ב",

            // נביאים אחרונים
            "ישעיהו", "ירמיהו", "יחזקאל",
            "הושע", "יואל", "עמוס", "עובדיה", "יונה", "מיכה",
            "נחום", "חבקוק", "צפניה", "חגי", "זכריה", "מלאכי",

            // כתובים
            "תהלים", "משלי", "איוב",
            "שיר השירים", "רות", "איכה", "קהלת", "אסתר",
            "דניאל", "עזרא", "נחמיה", "דברי הימים א", "דברי הימים ב",

            // תלמוד
             "ברכות", "שבת", "עירובין", "פסחים", "שקלים",
            "יומא", "סוכה", "ביצה", "ראש השנה", "תענית",
            "מגילה", "מועד קטן", "חגיגה", "יבמות", "כתובות",
            "נדרים", "נזיר", "סוטה", "גיטין", "קידושין",
            "בבא קמא", "בבא מציעא", "בבא בתרא", "סנהדרין",
            "מכות", "שבועות", "עבודה זרה", "הוריות", "זבחים",
            "מנחות", "חולין", "בכורות", "ערכין", "תמורה",
            "כריתות", "מעילה", "תמיד", "מדות", "קינים",
            "נידה"
         };

        static Regex htmlHeadersRegex = new Regex(@"^(\(([^( ]+)\)|<h([1-6])>)([^<(]+)");

        public static string CleanNonWordChars(string name)
           => Regex.Replace(name, @"[^\w -]", "").Trim();

        public static async Task LoadFilesContentHeaders(string rootPath, List<FileSystemItem> items)
        {
            var validItems = items
                .Where(item => item.IsFile && (item.Parent?.IsFile == false) &&
                   (item.Extension.ToLower().Contains("txt") || item.Extension.ToLower().Contains("html")));

            if (validItems.Count() <= 0) return;

            foreach (var item in validItems)
            {
                await LoadFileContent(rootPath, item, loadTagsOnly: true, useFullName: true);
            }
        }

        public static async Task<string> LoadFileContent(string rootPath, FileSystemItem fileSystemItem, bool loadTagsOnly, bool useFullName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            if (fileSystemItem.Children == null || !fileSystemItem.HasChildren)
            {
                fileSystemItem.Children = await Task.Run(() =>
                {
                    var headerTags = new List<FileSystemItem>();

                    using (var reader = new StreamReader(fileSystemItem.Path))
                    {
                        FileSystemItem htmlParent = fileSystemItem;
                        int index = -2;
                        while (!reader.EndOfStream)
                        {
                            index++;
                            string line = reader.ReadLine().Trim();

                            Match regexMatch = htmlHeadersRegex.Match(line.ToLower().Trim());
                            if (regexMatch.Success)
                            {
                                int level = 1;
                                string levelString = regexMatch.Groups[3].ToString();
                                if (int.TryParse(levelString, out int L)) level = L;

                                string name = regexMatch.Groups[4].ToString();

                                if (line.StartsWith("("))
                                {
                                    level = 7;
                                    name = regexMatch.Groups[2].ToString();
                                }

                                name = CleanNonWordChars(name);
                                if (name == fileSystemItem.Name) continue;

                                while (htmlParent.IsFile && level <= htmlParent.Level)
                                    htmlParent = htmlParent.Parent;

                                if (useFullName || htmlParent != fileSystemItem)
                                    name = $"{htmlParent.Name} {name}";

                                var newTagItem = new FileSystemItem(rootPath, fileSystemItem.Path, true, level, htmlParent, name, index);

                                if (htmlParent == fileSystemItem)
                                    headerTags.Add(newTagItem);
                                else
                                    htmlParent.Children.Add(newTagItem);

                                htmlParent = newTagItem;

                                if (!loadTagsOnly && level < 7)
                                    stringBuilder.AppendLine($"</section><section id=\"{name}\">");
                            }

                            if (!loadTagsOnly)                            
                                stringBuilder.AppendLine($"<line id=\"{index}\">{line} </line>");                             
                            
                        }

                    }

                    return headerTags;
                });
            }

            if (!loadTagsOnly) stringBuilder.AppendLine("</section>");
            return stringBuilder.ToString();
        }
    }
}
