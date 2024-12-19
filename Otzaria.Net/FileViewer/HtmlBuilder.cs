using DocumentFormat.OpenXml.Drawing.Charts;
using FileSystemBrowser;
using Lucene.Net.Search;
using System.IO;

namespace FileViewer
{
    public static class HtmlBuilder
    {
        public static string Build(string content, FileSystemItem fileSystemItem)
        {
            if (string.IsNullOrEmpty(content)) content = "<spinner/>";
            int index = fileSystemItem.IsFile ? fileSystemItem.Index : 1;
            return $@"
<!DOCTYPE html>
    <html>
        <head>
            <meta charset=UTF-8"">
            <title>{Path.GetFileName(fileSystemItem.Path)}</title>
            <style>
            
            body {{padding: 5px; line-height: 125%; text-align: justify; margin-top: 10px;}}
            
            line {{display: block; }}
            .line:hover {{ background-color: #f0f0f0;  cursor: default; }} /* when links are toggled line gets also .line style */  

            section_id {{position: fixed; background: #333;  color: #fff; opacity: 0.8; left: 0px; top:0px; }}

            .commentry {{background-color: #f0f4ff; padding: 10px; border-left: 3px solid #a1c4ff; }}
            .commentry header {{font-weight: bold; font-size: 90%; }}
            .commentry content {{margin-left: 5px;  margin-right: 5px;  font-size: 85%; }}

            </style>
        </head>
            <section_id id=""currentSection""></section_id>
            
            <body dir=""auto"">

            {content}

       <script>
            document.addEventListener('DOMContentLoaded', function() {{navigateToLine({{index}});}});

            let originalText = document.body.innerHTML;
            let isVowelsReversed = false;
            let isCantillationReversed = false;
            let isInline = false;
            let isLinkModeToggled = false;
            let isLinksToggled = false; 
            let isCommentriesToggled = false; 
            let currentSectionIndex = 0;

            const sections = document.querySelectorAll('section');
            const floating = document.getElementById('currentSection');

            const observer = new IntersectionObserver((entries) => {{
                entries.forEach(entry => {{
                    if (entry.isIntersecting) {{
                        floating.textContent = `${{entry.target.id}}`;
                        currentSectionIndex = Array.from(sections).indexOf(entry.target);
                    }}
                }});
            }});

            sections.forEach(section => {{
                observer.observe(section);
            }});

            function goBack() {{
                if (currentSectionIndex > 0) {{
                    currentSectionIndex--;
                    updateCurrentSection();
                }}
            }}
            
            function goForward() {{
                if (currentSectionIndex < sections.length - 1) {{
                    currentSectionIndex++;
                    updateCurrentSection();
                }}
            }}

             const updateCurrentSection = () => {{ 
                sections[currentSectionIndex].scrollIntoView({{ behavior: 'smooth', block: 'start' }});
            }};

            function navigateToLine(lineNumberString) {{
                const lineNumber = parseInt(lineNumberString, 10);
                const lines = document.querySelectorAll('line');

                if (isNaN(lineNumber) || lineNumber < 1 || lineNumber > lines.length) {{
                    console.log('Invalid line number');
                    return;
                }}

                const targetLine = lines[lineNumber];
                targetLine.scrollIntoView({{ block: 'center'}});
                
                const originalBackgroundColor = targetLine.style.backgroundColor;
                targetLine.style.backgroundColor = '#f5f5f5'; // Set highlight color

                setTimeout(() => {{
                    targetLine.style.backgroundColor = originalBackgroundColor || ''; // Restore original color
                }}, 5000); // Highlight duration: 2000ms (2 seconds)
            }};

            function toggleInline()
            {{
                const lines = document.querySelectorAll('line');
                isInline = !isInline;
                lines.forEach(line => {{
                    line.style.display = isInline ? 'inline' : 'block';
                }});
            }};

            function toggleLinkMode() {{
                document.querySelectorAll('line').forEach(element => {{
                    element.classList.toggle('line');
                
                    if (isLinkModeToggled)  {{ element.removeEventListener(""click"", handleLineLinkClick);}}
                    else  {{  element.addEventListener(""click"", handleLineLinkClick); }} 
                }});

                isLinkModeToggled = !isLinkModeToggled;
            }}

            function toggleLinks() {{  isLinksToggled = !isLinksToggled;}};
            function toggleCommentries() {{  isCommentriesToggled = !isCommentriesToggled;}};

            const handleLineLinkClick = (event) => {{
                const id = parseInt(event.target.id); // Parse ID from the clicked element
                if (isNaN(id)) {{ console.error(""Invalid ID:"", event.target.id); return; }}

                if (isLinksToggled && isLinkModeToggled) {{ window.chrome.webview.postMessage({{ ""getLinks"": id }}); }} 
                else if (isLinkModeToggled) {{ window.chrome.webview.postMessage({{ ""getCommentry"": id }}); }}
            }};


            </script>
        </body>
</html>";
        }
    }
}
