using HtmlAgilityPack;
using Lucene.Net.Search.Suggest.Jaspell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Otzaria.Net.Viewer
{
    public static class HtmlParser
    {
        public static string Parse(string filePath) 
        {
            StringBuilder writer = new StringBuilder();
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();
                    if (line.StartsWith("(")) line = Regex.Replace(line, @"\(([^(]+)\)", "<inline_Header>($1)</inline_Header>");
                    writer.AppendLine($"<span class=\"_line\" dir=\"auto\">{line}</span>");
                }
            }

            string html = Html(writer.ToString(), Path.GetFileName(filePath));
            html = HtmlTreeBuilder(html);
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(filePath)}.html");
            File.WriteAllText(tempFilePath, html);
            return tempFilePath;
        }

        static string Html(string content, string fileName)
        {
            return $@"<!DOCTYPE html>
<html><head><meta charset=UTF-8"">
    
    <title>{fileName}</title>
    <style>
       {Css()}
        
    </style>
</head>

<body dir=""auto"">

    <toolbar>
        <button title=""Navigation"" onclick=""toggleNavigationPanel()"">=</button>
    </toolbar>

    <main>
        <navcontainer id=""navcontainer"">
            <input id=""treeView-SearchInput"" type=""text"" onkeyup=""findAndSelectItem()"" placeholder=""חפש כותרת..."">
            
            <div class=""treeView"" id=""treeView"">             
</div>
        </navcontainer>

        <doccontent id=""contentBox"">
          {content}
        </doccontent>
    </main>

    <script>{Js()}</script>


</body></html>
";
        }

        static string Css()
        {
            return @"html, body {  
            display: flex;
            height: 100%;
            margin: 0;
            padding: 0;
            overflow-y: hidden;
            background-color: white;
        }

 /* Style for the loading animation container */
  #loading-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(255, 255, 255, 0.8);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
  }

  /* Simple spinner style */
  .spinner {
    border: 4px solid #f3f3f3; /* Light grey */
    border-top: 4px solid #3498db; /* Blue */
    border-radius: 50%;
    width: 50px;
    height: 50px;
    animation: spin 1s linear infinite;
  }

  /* Animation for the spinner */
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
        
        * {box-sizing: border-box; }

        main {
            margin-top: 35px;
            display: flex;
            height: calc(100% - 35px);
        }

        toolbar {
            border-bottom: 1px solid #ccc;
            background-color:#f9f9f9;
            position:fixed;
            display: flex;
            height: 35px;
            width: 100%;
        }

        toolbar button 
        {
            min-height: 35px;
            border: none;
            background-color: transparent;
        }

        toolbar button:hover { background-color: lightgray;}

        doccontent {
            padding-right: 10px;
            padding-left: 10px;
            text-align: justify;
            flex: 1;
            width: 100%;
            height: 100%;
            overflow-y: auto;
        }

        navcontainer {
            border-bottom: 1px solid #ccc;
            border-left: 1px solid #ccc;
            border-right: 1px solid #ccc;
            background: #f9f9f9;
            display: flex;
            flex-direction: column;
            height: 100%;
            max-width: 50%;
        }

        navcontainer input{
            margin:5px;
            padding: 3px;
            border: 0px solid #ccc;
        }

        .treeView {
user-select: none;
 height:100%;
  overflow: auto;
  margin-top: 5px;
  white-space: nowrap;
  text-indent: -40px;
}
 
.treeView details {
  border-top: 1px solid  #eaeaea;
  border-bottom: 1px solid  #eaeaea;
}
 
.treeView summary::-webkit-details-marker {
    display: none;
}
 
.treeView summary {
    transition: background-color 0.3s ease;
    list-style: none;
}
 
.treeView summary:hover {
  background-color: #eaeaea;
}
 
.treeView Button {
  background: none;
  border: none;
  cursor: pointer;
  font-weight: 500;
  margin: 5px;
  transition: background-color 0.3s ease;
  border-radius: 50px;
}
 
.treeView button:hover {
  background-color: #f9f9f9;
}

inline_Header {
 font-weight: bold;
 font-size: 90%;
}";
        }

        static string Js()
        {
            return @"window.onload = async function() {
    document.getElementById('loading-container').remove(); 
    toggleNavigationPanel();
  };

        function toggleNavigationPanel() 
        { 
            var element = document.getElementById('navcontainer');
            var searchInput = document.getElementById('navigation_SearchInput');
            if (element.style.maxWidth === '0%') 
            {
                element.style.maxWidth = ""50%"";
               
            }
            else 
            {
                element.style.maxWidth = ""0%"";
            } 
        }

  async function populateTreeView() {
  const contentBox = document.getElementById('contentBox');
  const treeView = document.getElementById('treeView');

  let currentDetails = treeView;
  let currentIndentLevel = 0;
 
  // Loop through each heading element in contentBox
  contentBox.querySelectorAll('h1, h2, h3, h4, h5, h6').forEach((heading, index) => {
    const indentLevel = parseInt(heading.tagName[1]);
 
    // Ensure the heading has a unique id
    if (!heading.id) {
      heading.id = `heading-${index}`;
    }
 
    // If the current heading has a lower or equal indent level to the previous one,
    // we need to move up the tree to the appropriate parent details element
    while (currentIndentLevel >= indentLevel) {
      currentDetails = currentDetails.parentElement;
      currentIndentLevel--;
    }
 
    // Create a new details and summary elements
    const details = document.createElement('details');
    const summary = document.createElement('summary');
    const button = document.createElement('button');
    if (indentLevel == 1) 
    {
        summary.style.paddingRight = 20 * 2 + 'px';
    }
    else
    {
           summary.style.paddingRight = 20 * indentLevel + 'px';
    }
    button.textContent = '👁';
    button.setAttribute('onclick', `treeViewSelection('${heading.id}')`);
    button.setAttribute('title', 'הצג');
    summary.appendChild(button);
    summary.appendChild(document.createTextNode(heading.textContent));
    details.appendChild(summary);
 
    // Append the new details element to the currentDetails
    currentDetails.appendChild(details);
 
    // Update the currentDetails and currentIndentLevel for the next iteration
    currentDetails = details;
    currentIndentLevel = indentLevel;
  });
}
 
function treeViewSelection(id) {
  // Scroll the corresponding heading into view
  const heading = document.getElementById(id);
  if (heading) {
    heading.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}
 

 
 
//
//treeView-Search
//
 
function findAndSelectItem() {
    var input = document.getElementById(""treeView-SearchInput"");
    var filter = input.value.trim().toUpperCase().replace(/,/g, '');
    var details = document.querySelectorAll(""details"");
    var firstMatchFound = false;
    
    // Collapse all details if filter is empty
    if (filter === """") {
         for (var i = 0; i < details.length; i++) {
        details[i].open = false;
        var summary = details[i].querySelector(""summary"");
        details[i].style.display = """";
    }
        return; // Exit function
    }
    
    for (var i = 0; i < details.length; i++) {
        var summary = details[i].querySelector(""summary"");
        if (summary) {
            var parentPath = getParentText(details[i]).replace(/👁/g, '').toUpperCase();
            var summaryPath = summary.textContent.replace(/👁/g, '').trim().toUpperCase();
            var fullPath = parentPath + "" "" + summaryPath;
            
            // Highlight matching summaries
            if (fullPath.includes(filter)) {
                 details[i].open = true;
                 details[i].style.display = """";
                 
                  if (!firstMatchFound) {
                    summary.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    firstMatchFound = true;
                }
                 
                 // Open parent details elements recursively
                var parentDetails = details[i].parentNode;
                while (parentDetails.tagName === 'DETAILS') {
                    parentDetails.open = true;
                    parentDetails.style.display = """";
                    parentDetails = parentDetails.parentNode;
                }
                
            } else {
                 details[i].open = false;
                   details[i].style.display = ""none"";
            }
        }
    }
}
 
function getParentText(element) {
    var text = """";
    var parent = element.parentNode;
    while (parent && parent.tagName.toLowerCase() === 'details') {
        var summary = parent.querySelector(""summary"");
        if (summary) {
            text = summary.textContent.trim() + "" "" + text;
        }
        parent = parent.parentNode;
    }
    return text.trim();
}";
        }

        static string HtmlTreeBuilder(string inputHtml)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(inputHtml);

            var contentBox = htmlDoc.GetElementbyId("contentBox");
            var treeView = htmlDoc.GetElementbyId("treeView");

            if (contentBox == null || treeView == null)
            {
                Debug.Print("Required elements ('contentBox' or 'treeView') not found.");
                return inputHtml;
            }
               
            var currentDetails = treeView;
            int currentIndentLevel = 0;

            // Get all heading tags in the contentBox
            var headings = contentBox.SelectNodes(".//h1|.//h2|.//h3|.//h4|.//h5|.//h6");
            if (headings == null) return inputHtml; // No headings found

            foreach (var (heading, index) in headings.Select((node, i) => (node, i)))
            {
                // Get heading level from the tag name
                int indentLevel = int.Parse(heading.Name.Substring(1)); // 'h1' -> 1, 'h2' -> 2, etc.

                // Ensure each heading has a unique id
                if (string.IsNullOrEmpty(heading.Id))
                {
                    heading.SetAttributeValue("id", $"heading-{index}");
                }

                // Adjust the parent element based on the heading level
                while (currentIndentLevel >= indentLevel)
                {
                    currentDetails = currentDetails.ParentNode; // Move up the tree
                    currentIndentLevel--;
                }

                // Create new details and summary elements
                var details = HtmlNode.CreateNode("<details></details>");
                var summary = HtmlNode.CreateNode("<summary></summary>");

                // Create the button and set its attributes
                var button = HtmlNode.CreateNode("<button></button>");
                button.InnerHtml = "👁";
                button.SetAttributeValue("onclick", $"treeViewSelection('{heading.Id}')");
                button.SetAttributeValue("title", "הצג");

                // Adjust summary padding
                summary.SetAttributeValue("style", $"padding-right: {20 * (indentLevel == 1 ? 2 : indentLevel)}px;");
                summary.AppendChild(button);
                summary.AppendChild(HtmlNode.CreateNode($"<span>{heading.InnerText}</span>"));

                // Append summary to details and details to currentDetails
                details.AppendChild(summary);
                currentDetails.AppendChild(details);

                // Update currentDetails and currentIndentLevel
                currentDetails = details;
                currentIndentLevel = indentLevel;
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }
    }
}
