using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using WordInterop = Microsoft.Office.Interop.Word;

namespace Otzaria.Net.Helpers
{
    internal class OfficeToHtml
    {
        public static async Task<string> Convert(string filePath)
        {
            await Task.Run(() => {
                if (Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    // Attempt conversion using WmlToHtmlConverter
                    try
                    {
                        var sourceDocxFileContent = File.ReadAllBytes(filePath);
                        using (var memoryStream = new MemoryStream())
                        {
                            memoryStream.Write(sourceDocxFileContent, 0, sourceDocxFileContent.Length);
                            using (var wordProcessingDocument = WordprocessingDocument.Open(memoryStream, false))
                            {
                                var settings = new WmlToHtmlConverterSettings();
                                var html = WmlToHtmlConverter.ConvertToHtml(wordProcessingDocument, settings);
                                var htmlString = html.ToString(SaveOptions.DisableFormatting);

                                // Save the HTML to a temp file
                                string tempHtmlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath) + "_Converted.html");
                                File.WriteAllText(tempHtmlPath, htmlString, Encoding.UTF8);

                                return tempHtmlPath;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("WmlToHtmlConverter failed: " + ex.Message);
                    }
                }

                // Fallback to WordInterop
                return ConvertUsingInterop(filePath);
            });

            return null;
        }

        private static string ConvertUsingInterop(string filePath)
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
                        Application.Current.Exit += (s, e) => { try { Marshal.ReleaseComObject(wordApp); } catch { } };
                    }
                    catch (COMException)
                    {
                        wordApp = new WordInterop.Application();
                        newApp = true;
                        Application.Current.Exit += (s, e) => { try { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); } catch { } };
                    }
                });

                WordInterop.Document wordDoc = wordApp.Documents.Open(filePath, Visible: false);
                wordDoc.SaveAs2(tempHtmlPath, WordInterop.WdSaveFormat.wdFormatFilteredHTML);
                wordDoc.Close(false);
            }
            finally
            {
                if (newApp && wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }

            if (File.Exists(tempHtmlPath))
            {
                return tempHtmlPath;
            }
            else
            {
                throw new FileNotFoundException("HTML conversion failed.");
            }
        }
    }
}
