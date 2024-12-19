using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;

class Program
{
    [STAThread] // Required for file picker dialog
    static void Main()
    {
            // Open File Picker
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx",
                Title = "Select a Word Document"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string wordFilePath = openFileDialog.FileName;
                Console.WriteLine($"Selected File: {wordFilePath}");

                // Generate HTML
                string htmlContent = ConvertWordToHtml(wordFilePath);

                // Save HTML to Desktop
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string htmlFilePath = Path.Combine(desktopPath, "ConvertedDocument.html");

                File.WriteAllText(htmlFilePath, htmlContent);

                Console.WriteLine($"HTML file saved to: {htmlFilePath}");
            }
            else
            {
                Console.WriteLine("No file selected. Exiting.");
            }
       

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static string ConvertWordToHtml(string wordFilePath)
    {
        byte[] fileBytes = File.ReadAllBytes(wordFilePath); // Load file into memory

        // Create an expandable MemoryStream
        using (MemoryStream memoryStream = new MemoryStream())
        {
            memoryStream.Write(fileBytes, 0, fileBytes.Length); // Write the file data into the MemoryStream
            memoryStream.Seek(0, SeekOrigin.Begin); // Reset the position to the beginning

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(memoryStream, true)) // Open in read-write mode
            {
                var settings = new HtmlConverterSettings
                {
                    PageTitle = "Converted Document"
                };

                XElement htmlElement = HtmlConverter.ConvertToHtml(wordDoc, settings);

                // Convert the XElement to a formatted HTML string
                return htmlElement.ToString();
            }
        }
    }


}
