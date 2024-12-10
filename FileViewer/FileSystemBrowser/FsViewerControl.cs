using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace FileSystemBrowser
{
    public class FsViewerControl : Control
    {
        public static readonly DependencyProperty RootItemProperty =DependencyProperty.Register(
              "RootItem", typeof(FileSystemItem), typeof(FsViewerControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
                "SelectedItem", typeof(FileSystemItem), typeof(FsViewerControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FsViewerControl)d;
            // Add any additional handling logic if necessary
        }


        public FileSystemItem RootItem
        {
            get => (FileSystemItem)GetValue(RootItemProperty);
            set => SetValue(RootItemProperty, value);
        }

        public FileSystemItem SelectedItem
        {
            get => (FileSystemItem)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
    }
}
