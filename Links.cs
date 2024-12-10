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

