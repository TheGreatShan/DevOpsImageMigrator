using System.Text.Json.Serialization;
using HtmlAgilityPack;

namespace AzureDevOpsImageMigrator.services;

public record QueryResult(
    [property: JsonPropertyName("queryType")]
    string QueryType,
    [property: JsonPropertyName("queryResultType")]
    string Query,
    [property: JsonPropertyName("asOf")] DateTime AsOf,
    [property: JsonPropertyName("columns")]
    List<Column> Columns,
    [property: JsonPropertyName("workItems")]
    List<WorkItem> WorkItems
);

public record WorkItem(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("url")] string Url
);

public record Column(
    [property: JsonPropertyName("referenceName")]
    string ReferenceName,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url);

public record WorkItemProperties([property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("fields")] Fields Fields);

public record Fields(
    [property: JsonPropertyName("Microsoft.VSTS.Common.AcceptanceCriteria")]
    string AcceptanceCriteria,
    [property: JsonPropertyName("System.Description")]
    string Description
);

internal static class ImageMigrator
{
    internal static List<string> GetImageLinks(this string html)
    {
        var imageLinks = new List<string>();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        var nodes = doc.DocumentNode.SelectNodes("//img");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var src = node.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(src))
                    imageLinks.Add(src);
            }
        }
        
        return imageLinks;
    }
}