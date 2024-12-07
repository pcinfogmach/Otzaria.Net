using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using WordInterop = Microsoft.Office.Interop.Word;

namespace Otzaria.Net.Helpers
{
    public static class HtmlConverter
    {
        public static string Convert(string filePath)
        {
            string tempHtmlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath) + "_FullTextExtractorTemp.html");

            WordInterop.Application wordApp = null;
            bool newApp = false;

            try
            {
                Application.Current.Dispatcher.Invoke(() => {
                    try
                    {
                        wordApp = (WordInterop.Application)Marshal.GetActiveObject("Word.Application");
                    }
                    catch (COMException)
                    {
                        wordApp = new WordInterop.Application();
                        newApp = true;
                    }
                });

                WordInterop.Document wordDoc = wordApp.Documents.Open(filePath, false, Visible: false);
                var html = wordDoc.Content.HTMLDivisions;
                File.WriteAllText(tempHtmlPath, html.ToString());
                wordDoc.Close(false);
            }
            finally
            {
                if (wordApp != null)
                {
                    if (newApp) wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }

            if (File.Exists(tempHtmlPath))
            {
                return tempHtmlPath;
            }
            else
            {
                return filePath;
            }
        }
    }
}
