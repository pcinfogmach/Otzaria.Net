using Microsoft.Web.WebView2.Wpf;
using Otzaria.Net.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyControls
{
    public class WebView2Base : WebView2
    {
        public static readonly DependencyProperty IsSelectedProperty =
           DependencyProperty.Register("IsSelected", typeof(bool), typeof(WebView2Base), new PropertyMetadata(true, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as WebView2Base;
            control?.UpdateState();
        }

        private async void UpdateState()
        {
            if (!IsSelected)
            {
                this.Visibility = Visibility.Hidden;
                if (!this.IsVisible && this.CoreWebView2 != null)
                    await this.CoreWebView2.TrySuspendAsync();
            }
            else
            {
                this.Visibility = Visibility.Visible;
                if (this.CoreWebView2 != null) 
                    this.CoreWebView2.Resume();
            }
        }

        public void SetBinding(TabItem tabItem) => BindingOperations.SetBinding(this, IsSelectedProperty,
           new Binding { Source = tabItem, Path = new PropertyPath("IsSelected"), Mode = BindingMode.TwoWay });

    }
}
