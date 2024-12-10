using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FileSystemBrowser
{
    public class FsViewerControl : Control
    {
        public static readonly DependencyProperty RootDirectoryProperty = DependencyProperty.Register(
              "RootDirectory", typeof(string), typeof(FsViewerControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty RootItemProperty =DependencyProperty.Register(
              "RootItem", typeof(FileSystemItem), typeof(FsViewerControl));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
                "SelectedItem", typeof(FileSystemItem), typeof(FsViewerControl));

        public FileSystemItem RootItem
        {
            get => (FileSystemItem)GetValue(RootItemProperty);
            set => SetValue(RootItemProperty, value);
        }

        public string RootDirectory
        {
            get => (string)GetValue(RootItemProperty);
            set => SetValue(RootItemProperty, value);
        }

        public FileSystemItem SelectedItem
        {
            get => (FileSystemItem)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
    }
}
