using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FileSystemBrowser.Models
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
    }

    public class FileSystemItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Tags { get; set; }
        public bool IsDirectory { get; set; }
        public int Level { get; set; } = -10;
        public bool HasChildren { get => Children.Count > 0; }
        public FileSystemItem Parent;
        public ObservableCollection<FileSystemItem> Children { get; set; } = new ObservableCollection<FileSystemItem>();

        public FileSystemItem(string rootDirectory, string path, bool isDirectory, int level, FileSystemItem parent = null, string name = "")
        {
            Path = path;
            Name = string.IsNullOrEmpty(name) ? System.IO.Path.GetFileNameWithoutExtension(path) : name;
            Tags = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetExtension(path))
            .Replace(rootDirectory, "")
                .Trim('\\')
                .Replace("\\", " \\ ");
            IsDirectory = isDirectory;
            Level = level;
            Parent = parent;
            
            if (isDirectory) LoadChildren(rootDirectory);
        }

        public override string ToString() => Name;

        void LoadChildren(string rootDirectory)
        {
            var directories = Directory.GetDirectories(Path)
                .OrderBy(dir =>
                {
                    var index = Array.IndexOf(FileSystemItemHelper.directoryOrder, System.IO.Path.GetFileName(dir));
                    return index == -1 ? int.MaxValue : index; // Unmatched items go to the end
                });

            foreach (var directory in directories)
            {
                Children.Add(new FileSystemItem(rootDirectory, directory, true, Level + 1, this));
            }

            var files = Directory.GetFiles(Path)
                .OrderBy(file =>
                {
                    var index = Array.IndexOf(FileSystemItemHelper.fileOrder, System.IO.Path.GetFileNameWithoutExtension(file));
                    return index == -1 ? int.MaxValue : index; // Unmatched items go to the end
                });

            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file).ToLower().Contains("txt") || System.IO.Path.GetExtension(file).ToLower().Contains("html"))
                    Children.Add(new HtmlFileSystemItem(rootDirectory, file, false, Level + 1, this));
                else 
                    Children.Add(new FileSystemItem(rootDirectory, file, false, Level + 1, this));
            }
        }
    }
}
