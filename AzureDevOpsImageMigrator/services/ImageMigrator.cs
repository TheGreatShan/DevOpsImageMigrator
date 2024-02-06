using System.Text.Json.Serialization;

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

internal static class ImageMigrator
{
}