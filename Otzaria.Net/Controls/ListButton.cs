using System.Diagnostics.Eventing.Reader;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Otzaria.Net.Controls
{
    internal class ListButton : Button
    {
        // Register the DependencyProperty
        public static readonly DependencyProperty ButtonFocusedProperty =
            DependencyProperty.Register(
                "ButtonFocused",
                typeof(bool),
                typeof(ListButton),
                new PropertyMetadata(false, OnButtonFocusedChanged));

        // Property wrapper for ButtonFocused DependencyProperty
        public bool ButtonFocused
        {
            get => (bool)GetValue(ButtonFocusedProperty);
            set => SetValue(ButtonFocusedProperty, value);
        }

        // Callback method when the ButtonFocused property changes
        private static void OnButtonFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as ListButton;
            if (button != null && (bool)e.NewValue)
            {
                // If ButtonFocused is true, set focus to the button
                button.Focus();
            }
        }


        public ListButton()
        {
            Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is TreeViewItem treeViewItem) { treeViewItem.Focus(); }
            else if (parent is ListViewItem listViewItem) { listViewItem.Focus(); }
            else if (parent is ListBoxItem listBoxItem) { listBoxItem.Focus(); }
        }
    }
}
