using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using Otzaria.Net.Models;

namespace Otzaria.Net.Controls
{
    internal class CostumeTreeView : TreeView
    {
        public CostumeTreeView()
        {
            PreviewKeyDown += CostumeTreeView_PreviewKeyDown;
            PreviewTouchDown += CostumeTreeView_PreviewTouchDown;
        }

        private void CostumeTreeView_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem treeViewItem)
            {
                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                e.Handled = true;
            }
        }

        private void CostumeTreeView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem treeViewItem)
            {
                if(e.Key == Key.Enter && treeViewItem.Items.Count > 0)
                {
                    treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                    e.Handled = true;
                }                   
            }          
        }
    }
}
