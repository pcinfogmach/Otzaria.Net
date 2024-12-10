using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Runtime.CompilerServices;
using FileSystemBrowser;

namespace MyHelpers
{
    public class TextBoxFocusBehavior
    {
        public static bool GetSelectAll(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(SelectAllProperty);
        }

        public static void SetSelectAll(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(SelectAllProperty, value);
        }

        public static bool GetCaptureFocus(FrameworkElement frameworkElement)
        {
            return (bool)frameworkElement.GetValue(CaptureFocusProperty);
        }

        public static void SetCaptureFocus(FrameworkElement frameworkElement, bool value)
        {
            frameworkElement.SetValue(CaptureFocusProperty, value);
        }

        public static readonly DependencyProperty CaptureFocusProperty =
            DependencyProperty.RegisterAttached("CaptureFocus",
                typeof(bool), typeof(TextBoxFocusBehavior),
                new FrameworkPropertyMetadata(false, OnCaptureFocusChanged));

        public static readonly DependencyProperty SelectAllProperty =
                 DependencyProperty.RegisterAttached("SelectAll",
                    typeof(bool), typeof(TextBoxFocusBehavior),
                    new FrameworkPropertyMetadata(false, OnSelectAllChanged));

        private static void OnSelectAllChanged
                   (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = d as FrameworkElement;
            if (frameworkElement == null) return;

            if (e.NewValue is bool == false) return;

            if ((bool)e.NewValue)
            {
                frameworkElement.GotFocus += SelectAll;
                frameworkElement.PreviewMouseDown += IgnoreMouseButton;
            }
            else
            {
                frameworkElement.GotFocus -= SelectAll;
                frameworkElement.PreviewMouseDown -= IgnoreMouseButton;
            }
        }

        private static void OnCaptureFocusChanged
                  (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var frameworkElement = d as FrameworkElement;
            if (frameworkElement == null) return;

            if (e.NewValue is bool == false) return;

            if ((bool)e.NewValue)
            {
                frameworkElement.Loaded += FrameworkElement_Loaded;
            }
            else
            {
                frameworkElement.Loaded -= FrameworkElement_Loaded;
            }
        }

        private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            frameworkElement.Focus();
        }

        private static void SelectAll(object sender, RoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement is TextBox)
                ((TextBoxBase)frameworkElement).SelectAll();
            else if (frameworkElement is PasswordBox)
                ((PasswordBox)frameworkElement).SelectAll();
        }

        private static void IgnoreMouseButton
                (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null || frameworkElement.IsKeyboardFocusWithin) return;
            e.Handled = true;
            frameworkElement.Focus();
        }
    }
}
