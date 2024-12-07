using Microsoft.Web.WebView2.Wpf;
using Otzaria.Net.Helpers;
using Otzaria.Net.Viewer;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Otzaria.Net.Controls
{
    public class OtzariaFileViewer : WebView2
    {
        private static readonly string[] WordDocumentExtensions = new string[]{
        ".doc", ".docm", ".docx", ".dotx", ".dotm", ".dot", ".odt", ".rtf" };

        public OtzariaFileViewer() 
        {
            Application.Current.Exit += (s, e) => { this.Dispose(); };
        }

        public void LoadFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            string extension = Path.GetExtension(path).ToLower();
            if (extension.EndsWith("pdf"))
            {
                this.Source = new Uri(path);
            }
            //else if (WordDocumentExtensions.Contains(extension))
            //{
            //    var htmlPath = HtmlConverter.Convert(path);
            //    this.Source = new Uri(HtmlFile(htmlPath));
            //}
            else if (extension.EndsWith("txt") || extension.EndsWith("html"))
            {
                this.Source = new Uri(HtmlParser.Parse(path));
            }          
        }
    }
}
