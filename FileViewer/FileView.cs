using System.Windows;
using System.Windows.Input;
using FileSystemBrowser;
using Microsoft.Web.WebView2.Wpf;
using MyHelpers;

namespace FileViewer
{
    public class FileView : WebView2
    {
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(int), typeof(FileView), new PropertyMetadata(100, OnFontSizeChanged));

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
                       "FontFamily", typeof(string), typeof(FileView), new PropertyMetadata("Arial", OnFontFamilyChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
              "SelectedItem", typeof(FileSystemItem), typeof(FileView), new PropertyMetadata(null, OnSelectedItemChanged));


        public int FontSize { get => (int)GetValue(FontSizeProperty);  set => SetValue(FontSizeProperty, value);}
        public string FontFamily  { get => (string)GetValue(FontFamilyProperty);  set => SetValue(FontFamilyProperty, value);}
        public FileSystemItem SelectedItem { get => (FileSystemItem)GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value);}

        async static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           if (d is FileView fileView && fileView.CoreWebView2 != null)
                await fileView.ExecuteScriptAsync($"document.body.style.fontSize = '{e.NewValue.ToString()}%';");
        }

        async static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView fileView && fileView.CoreWebView2 != null)
                await fileView.ExecuteScriptAsync($"document.body.style.fontFamily = '{e.NewValue}';");
        }

        async static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView fileView && fileView.CoreWebView2 != null && e.NewValue is HtmlFileSystemItem item)
                await fileView.ExecuteScriptAsync($"navigateToLine('{item.Index.ToString()}')");
        }

        public ICommand ToggleBlockInlineCommand { get; }
        public ICommand ToggleCantillationsCommand { get; }
        public ICommand ToggleNikudCommand { get; }
        public ICommand TogglePunctuationCommand { get; }

        public FileView()
        {
            ToggleBlockInlineCommand = new RelayCommand(ToggleBlockInline);
            ToggleCantillationsCommand = new RelayCommand(ToggleCantillations);
            ToggleNikudCommand = new RelayCommand(ToggleNikud);
            TogglePunctuationCommand = new RelayCommand(TogglePunctuation);
        }

        async void ToggleBlockInline()
        {
            if (this.CoreWebView2 != null)
                await this.ExecuteScriptAsync($"toggleInline();");
        }

        async void ToggleNikud()
        {
            if (this.CoreWebView2 != null)
                await this.ExecuteScriptAsync($@"
                var newText = originalText;
                if (!isVowelsReversed)
                {{
                    newText = newText.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5,;?!.:]/g, """");
                }}           
                if (isCantillationReversed)
                {{
                    newText = newText.replace(/[\u0591-\u05AF]/g, """");
                }}   

                document.body.innerHTML = newText
                isVowelsReversed = !isVowelsReversed;");
        }

        async void ToggleCantillations()
        {
            if (this.CoreWebView2 != null)
                await this.ExecuteScriptAsync($@"
                var newText = originalText;
                if (!isCantillationReversed)
                {{
                    newText = newText.replace(/[\u0591-\u05AF]/g, """");
                }}
                if (isVowelsReversed)
                {{
                    newText = newText.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5]/g, """");
                }}          
                document.body.innerHTML = newText
                isCantillationReversed = !isCantillationReversed;");
        }

        async void TogglePunctuation()
        {
            throw new System.NotImplementedException();
            //if (this.CoreWebView2 != null)
            //    await this.ExecuteScriptAsync($@"
            //    var newText = originalText;
            //    if (!isCantillationReversed)
            //    {{
            //        newText = newText.replace(/[,;?!.:]/g, """");
            //    }}
            //    if (isVowelsReversed)
            //    {{
            //        newText = newText.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5]/g, """");
            //    }}          
            //    document.body.innerHTML = newText
            //    isCantillationReversed = !isCantillationReversed;");
        }
    }
}
