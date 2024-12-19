using System;
using System.Windows;
using System.Windows.Media;

namespace Otzaria.Net.Helpers
{
    public static class MyVisualTreeHelper
    {
        public static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            if (child == null)
                return null;

            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }
    }
}
