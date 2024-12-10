using System;

public class Class1
{
	public Class1()
	{
        async void ToggleNikud()
        {
            if (this.CoreWebView2 != null)
                await this.ExecuteScriptAsync($@"
        var newText = originalText;
        
        if (!isVowelsReversed)
        {{
            newText = newText.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5]/g, """");
        }}
        if (isCantillationReversed)
        {{
            newText = newText.replace(/[\u0591-\u05AF]/g, """");
        }}
        if (isPunctuationReversed)
        {{
            newText = newText.replace(/[,;?!.:]/g, """");
        }}
        
        document.body.innerHTML = newText;
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
        if (isPunctuationReversed)
        {{
            newText = newText.replace(/[,;?!.:]/g, """");
        }}
        
        document.body.innerHTML = newText;
        isCantillationReversed = !isCantillationReversed;");
        }

        async void TogglePunctuation()
        {
            if (this.CoreWebView2 != null)
                await this.ExecuteScriptAsync($@"
        var newText = originalText;
        
        if (!isPunctuationReversed)
        {{
            newText = newText.replace(/[,;?!.:]/g, """");
        }}
        if (isVowelsReversed)
        {{
            newText = newText.replace(/[\u05B0-\u05BD\u05C1\u05C2\u05C4\u05C5]/g, """");
        }}
        if (isCantillationReversed)
        {{
            newText = newText.replace(/[\u0591-\u05AF]/g, """");
        }}
        
        document.body.innerHTML = newText;
        isPunctuationReversed = !isPunctuationReversed;");
        }

    }
}
