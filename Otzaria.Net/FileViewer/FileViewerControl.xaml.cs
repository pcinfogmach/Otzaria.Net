﻿using FileSystemBrowser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileViewer
{
    /// <summary>
    /// Interaction logic for FileViewerControl.xaml
    /// </summary>
    public partial class FileViewerControl : UserControl
    {
        public FileViewerControl()
        {
            InitializeComponent();
            LoadItem("C:\\אוצריא\\אוצריא\\תנך\\תורה\\בראשית.txt");
        }

        async void LoadItem(string path)
        {
            var rootItem = new HtmlFileSystemItem(path, path, false, 0);
            string content = await rootItem.LoadContent(path, false, false);

            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(path)}.html");
            string html = HtmlBuilder.Build(content, Path.GetFileName(path));
            File.WriteAllText(tempFilePath, html);

            fileView.Source = new Uri(tempFilePath);
            FsChapterViewer.RootItem = rootItem;
        }
    }
}