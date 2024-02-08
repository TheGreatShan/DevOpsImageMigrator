using System.Reflection;
using System.Text;
using System.Text.Json;
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

public record Images(int OldId, string Url);
internal static class ImageMigrator
{
    internal static List<Images> GetImages(this HttpClient client, QueryResult queryResult)
    {
        var imageLinks = new List<Images>();
        foreach (var workitem in queryResult.WorkItems)
        {
            var workItemProperties =
                JsonSerializer
                    .Deserialize<WorkItemProperties>(client.GetAsync(workitem.Url)
                        .Result
                        .Content
                        .ReadAsStringAsync()
                        .Result);

            if (workItemProperties?.Fields.AcceptanceCriteria is not null)
                workItemProperties.Fields.AcceptanceCriteria.GetImageLinks().ForEach(x => imageLinks.Add(new(
                    workitem.Id, x)));

            if (workItemProperties?.Fields.Description is not null)
                workItemProperties.Fields.Description.GetImageLinks().ForEach(x => imageLinks.Add(new (workitem.Id, x)));
        }

        return imageLinks;
    }
    
    internal static QueryResult? GetWorkItems(this HttpClient client, AppSettings appSettings)
    {
        var query = new
        {
            query = "Select [System.Id], [System.Title], [System.State] From WorkItems"
        };

        var content = JsonSerializer.Serialize(query);
        var result = client.PostAsync($"{appSettings.FromUrl}_apis/wit/wiql?api-version=6.0",
            new StringContent(content, Encoding.UTF8, "application/json")).Result;
        var queryResult = JsonSerializer.Deserialize<QueryResult>(result.Content.ReadAsStringAsync().Result);
        return queryResult;
    }
    
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