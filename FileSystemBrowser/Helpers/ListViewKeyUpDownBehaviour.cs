using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FileSystemBrowser.Helpers
{
    public class ListBoxKeyUpDownBehaviour
    {
        public static bool GetEnable(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(EnableProperty);
        }

        public static void SetEnable(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(EnableProperty, value);
        }

        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached("Enable",
                typeof(bool), typeof(ListBoxKeyUpDownBehaviour),
                new FrameworkPropertyMetadata(false, OnEnableChanged));

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement frameworkElement)
            {
                if ((bool)e.NewValue)
                {
                    frameworkElement.PreviewKeyDown += FrameworkElement_PreviewKeyDown;
                }
                else
                {
                    frameworkElement.PreviewKeyDown -= FrameworkElement_PreviewKeyDown;
                }
            }
        }

        private static void FrameworkElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                int selectedIndex = listBox.SelectedIndex;

                if (e.Key == Key.Down)
                {
                    // Move to the next item
                    if (selectedIndex < listBox.Items.Count - 1)
                    {
                        listBox.SelectedIndex = ++selectedIndex;
                        FocusButtonInItem(listBox, selectedIndex);
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Up)
                {
                    // Move to the previous item
                    if (selectedIndex > 0)
                    {
                        listBox.SelectedIndex = --selectedIndex;
                        FocusButtonInItem(listBox, selectedIndex);
                        e.Handled = true;
                    }
                }
            }
        }

        private static void FocusButtonInItem(ListBox listBox, int index)
        {
            if (listBox.ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem listBoxItem)
            {
                listBoxItem.Focus(); // Focus the ListBoxItem first
                listBoxItem.IsSelected = true;

                // Use visual tree helper to find the Button
                Button button = FindChildButton(listBoxItem);
                if (button != null)
                {
                    button.Focus(); // Set focus to the button
                   
                }
            }
        }

        private static Button FindChildButton(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is Button button)
                {
                    return button;
                }

                Button result = FindChildButton(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
