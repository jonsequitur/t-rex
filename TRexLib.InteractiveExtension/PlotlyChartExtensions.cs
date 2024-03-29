﻿using System.Text;
using Microsoft.AspNetCore.Html;
using XPlot.Plotly;
using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace TRexLib.InteractiveExtension
{
    public static class PlotlyChartExtensions
    {
        public static IHtmlContent GetHtmlContent(this PlotlyChart chart)
        {
            var divElement = div[style: $"width: {chart.Width}px; height: {chart.Height}px;", id: chart.Id]();
            var jsElement = chart.GetInlineJS().Replace("<script>", string.Empty).Replace("</script>", string.Empty);

            return new HtmlString($@"{divElement}
{GetScriptElementWithRequire(jsElement)}");
        }

        private static IHtmlContent GetScriptElementWithRequire(string script)
        {
            var newScript = new StringBuilder();
            newScript.AppendLine("<script type=\"text/javascript\">");
            newScript.AppendLine(@"
var renderPlotly = function() {
    var xplotRequire = requirejs.config({context:'xplot-3.0.1',paths:{plotly:'https://cdn.plot.ly/plotly-1.49.2.min'}});
    xplotRequire(['plotly'], function(Plotly) {");

            newScript.AppendLine(script);
            newScript.AppendLine(@"});
};
if ((typeof(requirejs) !==  typeof(Function)) || (typeof(requirejs.config) !== typeof(Function))) { 
    var script = document.createElement(""script""); 
    script.setAttribute(""src"", ""https://cdnjs.cloudflare.com/ajax/libs/require.js/2.3.6/require.min.js""); 
    script.onload = function(){
        renderPlotly();
    };
    document.getElementsByTagName(""head"")[0].appendChild(script); 
}
else {
    renderPlotly();
}");
            newScript.AppendLine("</script>");
            return new HtmlString(newScript.ToString());
        }
    }
}