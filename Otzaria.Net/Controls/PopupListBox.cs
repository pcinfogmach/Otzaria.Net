using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MyControls
{
    public class PopupListBox :ListBox
    {
        public PopupListBox()
        {
            SelectionChanged += PopupListBox_SelectionChanged;
        }

        private void PopupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Parent is Popup popup)
            {
                popup.IsOpen = false;

                if (popup.PlacementTarget is FrameworkElement element) 
                    element.Focus(); 
            }
        }
    }
}
