using FileSystemBrowser;
using MyControls;
using MyModels;
using Otzaria.Net.FileViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace FileViewer
{
    public class FileView : WebView2Base
    {
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof(int), typeof(FileView), new PropertyMetadata(100, OnFontSizeChanged));

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
                       "FontFamily", typeof(string), typeof(FileView), new PropertyMetadata("Arial", OnFontFamilyChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
              "SelectedItem", typeof(FileSystemItem), typeof(FileView), new PropertyMetadata(null, OnSelectedItemChanged));

        public static readonly DependencyProperty ToggleLinkModeProperty = DependencyProperty.Register(
           "ToggleLinkMode", typeof(bool), typeof(FileView), new PropertyMetadata(false, OnToggleLinkModeChanged));

        public static readonly DependencyProperty ToggleLinksProperty = DependencyProperty.Register(
            "ToggleLinks", typeof(bool), typeof(FileView), new PropertyMetadata(false, OnToggleLinksChanged));

        public static readonly DependencyProperty CurrentLinkIdProperty = DependencyProperty.Register(
            "CurrentLinkId", typeof(string), typeof(FileView));

        FileSystemItem _fileSystemItem;
        CancellationTokenSource _loadLinksCancellationSource;
        string filePath;

        public int FontSize { get => (int)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
        public bool ToggleLinkMode { get => (bool)GetValue(ToggleLinkModeProperty); set => SetValue(ToggleLinkModeProperty, value); }
        public bool ToggleLinks { get => (bool)GetValue(ToggleLinksProperty); set => SetValue(ToggleLinksProperty, value); }
        public FileSystemItem SelectedItem { get => (FileSystemItem)GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
        public string CurrentLinkId { get => (string)GetValue(CurrentLinkIdProperty); set => SetValue(FontFamilyProperty, value); }

        async static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView fileView && fileView.CoreWebView2 != null)
                await fileView.ExcuteScriptSafelyAsync($"document.body.style.fontSize = '{e.NewValue.ToString()}%';");
        }

        async static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView fileView && fileView.CoreWebView2 != null)
                await fileView.ExcuteScriptSafelyAsync($"document.body.style.fontFamily = '{e.NewValue}';");
        }

        async static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView fileView && fileView.CoreWebView2 != null && e.NewValue is FileSystemItem item && item.IsFile)
                await fileView.ExcuteScriptSafelyAsync($"navigateToLine('{(item.Index > 0 ? item.Index.ToString() : "1")}')");
        }

        async static void OnToggleLinkModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView fileView && fileView.CoreWebView2 != null)
                await fileView.ExcuteScriptSafelyAsync("toggleLinkMode();");
        }

        async static void OnToggleLinksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileView fileView && fileView.CoreWebView2 != null)
                await fileView.ExcuteScriptSafelyAsync("toggleLinks();");
        }

        public LinksViewModel LinksViewModel { get; set; } = new LinksViewModel();

        public ICommand ToggleBlockInlineCommand  => new RelayCommand(ToggleBlockInline);
        public ICommand ToggleCantillationsCommand => new RelayCommand(ToggleCantillations);
        public ICommand ToggleNikudCommand => new RelayCommand(ToggleNikud);
        public ICommand GoToNextCommand => new RelayCommand(GoToNextSection);
        public ICommand GoToPreviousCommand => new RelayCommand(GoToPreviousSection);

        public FileView()
        {
            Application.Current.Exit += (s, e) => { this.Dispose(); };
            WebMessageReceived += FileView_WebMessageReceived;
        }

        private async void FileView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = JsonSerializer.Deserialize<Dictionary<string, object>>(e.WebMessageAsJson);
            if (message == null) return;

            try
            {
                if (message.TryGetValue("getLinks", out var linkLineIndex) && linkLineIndex != null && double.TryParse(linkLineIndex.ToString(), out double linkIndex))
                {
                    _loadLinksCancellationSource?.Cancel();
                    _loadLinksCancellationSource = new CancellationTokenSource();
                    try { await LinksViewModel.LoadLinks(linkIndex +2, _loadLinksCancellationSource.Token); } catch (TaskCanceledException) { Debug.WriteLine("Operation was canceled."); }
                }
                else if (message.TryGetValue("getCommentry", out var commentryLineIndex) && double.TryParse(commentryLineIndex.ToString(), out double commentryIndex))
                {
                    _loadLinksCancellationSource?.Cancel();
                    _loadLinksCancellationSource = new CancellationTokenSource();
                    try
                    {
                        var result = await LinksViewModel.LoadCommentries(commentryIndex + 1, _loadLinksCancellationSource.Token);
                        return;
                    }
                    catch (TaskCanceledException) { Debug.WriteLine("Operation was canceled."); }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public void NavigateToContent(FileSystemItem fileSystemItem, string content, int lineIndex)
        {
            string html = HtmlBuilder.Build(content, fileSystemItem);

            this.filePath = fileSystemItem.Path;
            LinksViewModel.Path = filePath;

            string tempFilePath = Path.Combine(Path.GetTempPath(), "Otzaria.Net" + ".html");
            File.WriteAllText(tempFilePath, html);

            Source = new Uri(tempFilePath);

            this.Focus();
        }

        async void ToggleBlockInline()
        {
                await ExcuteScriptSafelyAsync($"toggleInline();");
        }

        async void ToggleNikud()
        {
                await ExcuteScriptSafelyAsync($@"
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
                await ExcuteScriptSafelyAsync($@"
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

        async void GoToNextSection() => await ExcuteScriptSafelyAsync(@"goForward()");

        async void GoToPreviousSection() => await ExcuteScriptSafelyAsync(@"goBack()");

        async Task ExcuteScriptSafelyAsync(string script) 
        {
            if (this.CoreWebView2 != null)
                await this.ExecuteScriptAsync(script);
        }
    }
}
