using Microsoft.Web.WebView2.Wpf;
using MyHelpers;
using MyModels;
using Otzaria.Net.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MyControls
{
    public class CostumeTabControl : TabControl
    {
        public static readonly DependencyProperty ShowItemsListProperty = DependencyProperty.Register("ShowItemsList", typeof(bool), typeof(CostumeTabControl));
        public bool ShowItemsList { get => (bool)GetValue(ShowItemsListProperty); set => SetValue(ShowItemsListProperty, value); }

        public ICommand CloseTabCommand => new RelayCommand<TabItem>(CloseTab);

        public void CloseTab(TabItem tabItem)
        {
            int tabIndex = this.Items.IndexOf(tabItem);
            if (tabIndex != this.SelectedIndex) tabIndex = -1;

            this.Items.Remove(tabItem);

            if (tabItem.Content is WebView2 webView) webView.Dispose();

            if (tabIndex != -1) this.SelectedIndex = tabIndex >= this.Items.Count ? tabIndex - 1 : tabIndex;
        }

        public CostumeTabControl() 
        {
            //var Style = (Style)Application.Current.Resources["PlaceHolderTextBox"];
            SelectionChanged += (s, e) => { ShowItemsList = false; };
        }
    }
}
