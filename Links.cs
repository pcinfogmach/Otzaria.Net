using System;
using System.Text.Json;

public class Link
{
    public double LineIndex1 { get; set; }
    public string HeRef2 { get; set; }
    public string Path2 { get; set; }
    public double LineIndex2 { get; set; }
    public string ConnectionType { get; set; }
}

public class Program
{
    public static void Main()
    {
        // JSON לדוגמה
        string jsonString = @"{
            ""line_index_1"": 2350.0,
            ""heRef_2"": ""רשב\""ם על בבא בתרא ק., א, א,"",
            ""path_2"": ""אוצריא\\תלמוד\\בבלי\\ראשונים על התלמוד\\רשבם\\סדר נזיקין\\רשבם על בבא בתרא.txt"",
            ""line_index_2"": 4758.0,
            ""Conection Type"": ""commentary""
        }";

        // Deserialize את ה-JSON לאובייקט
        Link link = JsonSerializer.Deserialize<Link>(jsonString);

        // הדפסת הערכים
        Console.WriteLine($"Line Index 1: {link.LineIndex1}");
        Console.WriteLine($"Hebrew Reference 2: {link.HeRef2}");
        Console.WriteLine($"Path 2: {link.Path2}");
        Console.WriteLine($"Line Index 2: {link.LineIndex2}");
        Console.WriteLine($"Connection Type: {link.ConnectionType}");
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<string> typeList = new List<string>();

            WindowState = WindowState.Minimized;
            var files = Directory.GetFiles(@"C:\אוצריא\links", "*", SearchOption.AllDirectories).ToList();

            int index = 0;
            Parallel.ForEach(files, file =>
            {
                var json = File.ReadAllText(file).Replace("Conection Type", "Conection_Type");
                var links = JsonSerializer.Deserialize<LinkItem[]>(json).GroupBy(l => l.Conection_Type);
                Console.WriteLine(files.Count + " \\ " + index++);

                lock (typeList)
                {
                    foreach (var group in links)
                    {
                        if (!typeList.Contains(group.Key))
                            typeList.Add(group.Key);
                    }
                }
            });

            File.WriteAllText("connectionTypes", string.Join("\r\n", typeList));
        }

        class LinkItem
        {
            public double line_index_1 { get; set; }
            public string heRef_2 { get; set; }
            public string path_2 { get; set; }
            public double line_index_2 { get; set; }
            public string Conection_Type { get; set; }
        }
    }
}

