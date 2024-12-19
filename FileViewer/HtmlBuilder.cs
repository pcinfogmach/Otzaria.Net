using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FileViewer
{
    public static class HtmlBuilder
    {
        public static string Build(string content, string fileName)
        {
            return $@"
<!DOCTYPE html>
    <html>
        <head>
            <meta charset=UTF-8"">
            <title>{fileName}</title>
            <style>
                body {{padding: 5px; line-height: 125%; text-align: justify; }}
                line {{display: block; }}
                .line:hover {{ background - color: #f0f0f0;  cursor: default; padding: 0.2px; }}
            </style>
        </head>

        <body dir=""auto"">
            {content}
            <script>
        
        let originalText = document.body.innerHTML;
        let isVowelsReversed = false;
        let isCantillationReversed = false;
        let isLinksMode = false;
        let isInline = false;

        function ToggleIsLinksMode()
        {{
            isLinksMode = !isLinksMode;
            lines.class.style.hover 
            lones.click send messge to c#
            }});
        }};
        
        function toggleInline()
        {{
            const lines = document.querySelectorAll('line');
            isInline = !isInline;
            lines.forEach(line => {{
                line.style.display = isInline ? 'inline' : 'block';
            }});
        }};

        function navigateToLine(lineNumberString) {{
            const lineNumber = parseInt(lineNumberString, 10);
            const lines = document.querySelectorAll('line');

            if (isNaN(lineNumber) || lineNumber < 1 || lineNumber > lines.length) {{
                alert('Invalid line number');
                return;
            }}

            const targetLine = lines[lineNumber - 1];
            targetLine.scrollIntoView({{ behavior: 'smooth', block: 'center' }});
            targetLine.style.backgroundColor = 'whitesmoke';

            setTimeout(() => {{
                targetLine.style.backgroundColor = '';
            }}, 2000);
        }};

            </script>
        </body>
</html>";
        }
    }
}
