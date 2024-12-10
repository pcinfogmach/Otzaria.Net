using System;

public class FileView : WebView2
{
    // Dependency Property for Font Size Percentage
    public static readonly DependencyProperty FontSizePercentageProperty =
        DependencyProperty.Register(
            "FontSizePercentage",
            typeof(double),
            typeof(FileView),
            new PropertyMetadata(100.0, OnFontSizePercentageChanged));

    // Dependency Property for Font Name
    public static readonly DependencyProperty FontNameProperty =
        DependencyProperty.Register(
            "FontName",
            typeof(string),
            typeof(FileView),
            new PropertyMetadata("Arial", OnFontNameChanged));

    // Dependency Property for Hebrew Cantillations visibility
    public static readonly DependencyProperty ShowHebrewCantillationsProperty =
        DependencyProperty.Register(
            "ShowHebrewCantillations",
            typeof(bool),
            typeof(FileView),
            new PropertyMetadata(false, OnShowHebrewCantillationsChanged));

    // Dependency Property for Hebrew Nikkud visibility
    public static readonly DependencyProperty ShowHebrewNikkudProperty =
        DependencyProperty.Register(
            "ShowHebrewNikkud",
            typeof(bool),
            typeof(FileView),
            new PropertyMetadata(false, OnShowHebrewNikkudChanged));

    // Dependency Property for Non-Word Characters visibility
    public static readonly DependencyProperty ShowNonWordCharsProperty =
        DependencyProperty.Register(
            "ShowNonWordChars",
            typeof(bool),
            typeof(FileView),
            new PropertyMetadata(false, OnShowNonWordCharsChanged));

    public FileView()
    {
        this.NavigationCompleted += OnNavigationCompleted;
    }

    // Font Size Percentage property
    public double FontSizePercentage
    {
        get => (double)GetValue(FontSizePercentageProperty);
        set => SetValue(FontSizePercentageProperty, value);
    }

    // Font Name property
    public string FontName
    {
        get => (string)GetValue(FontNameProperty);
        set => SetValue(FontNameProperty, value);
    }

    // Show Hebrew Cantillations property
    public bool ShowHebrewCantillations
    {
        get => (bool)GetValue(ShowHebrewCantillationsProperty);
        set => SetValue(ShowHebrewCantillationsProperty, value);
    }

    // Show Hebrew Nikkud property
    public bool ShowHebrewNikkud
    {
        get => (bool)GetValue(ShowHebrewNikkudProperty);
        set => SetValue(ShowHebrewNikkudProperty, value);
    }

    // Show Non-Word Characters property
    public bool ShowNonWordChars
    {
        get => (bool)GetValue(ShowNonWordCharsProperty);
        set => SetValue(ShowNonWordCharsProperty, value);
    }

    private static void OnFontSizePercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var webView = (FileView)d;
        webView.UpdateFontSize((double)e.NewValue);
    }

    private static void OnFontNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var webView = (FileView)d;
        webView.UpdateFontName((string)e.NewValue);
    }

    private static void OnShowHebrewCantillationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var webView = (FileView)d;
        webView.ToggleHebrewCantillations((bool)e.NewValue);
    }

    private static void OnShowHebrewNikkudChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var webView = (FileView)d;
        webView.ToggleHebrewNikkud((bool)e.NewValue);
    }

    private static void OnShowNonWordCharsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var webView = (FileView)d;
        webView.ToggleNonWordChars((bool)e.NewValue);
    }

    private void OnNavigationCompleted(object sender, EventArgs e)
    {
        // Inject JavaScript for the toggle display mode functionality
        string script = @"
                let isInline = false;
                document.getElementById('toggleDisplayButton').addEventListener('click', () => {
                    const lines = document.querySelectorAll('line');
                    isInline = !isInline; // Toggle the mode
                    lines.forEach(line => {
                        line.style.display = isInline ? 'inline' : 'block';
                    });
                });
            ";
        ExecuteScriptAsync(script);
    }

    private void UpdateFontSize(double percentage)
    {
        double fontSize = 16 * (percentage / 100); // Base font size 16
        string script = $"document.body.style.fontSize = '{fontSize}px';";
        ExecuteScriptAsync(script);
    }

    private void UpdateFontName(string fontName)
    {
        string script = $"document.body.style.fontFamily = '{fontName}';";
        ExecuteScriptAsync(script);
    }

    private void ToggleHebrewCantillations(bool show)
    {
        string script = show ?
            "document.body.classList.add('show-cantillations');" :
            "document.body.classList.remove('show-cantillations');";
        ExecuteScriptAsync(script);
    }

    private void ToggleHebrewNikkud(bool show)
    {
        string script = show ?
            "document.body.classList.add('show-nikkud');" :
            "document.body.classList.remove('show-nikkud');";
        ExecuteScriptAsync(script);
    }

    private void ToggleNonWordChars(bool show)
    {
        string script = show ?
            "document.body.classList.add('show-non-word-chars');" :
            "document.body.classList.remove('show-non-word-chars');";
        ExecuteScriptAsync(script);

    }
