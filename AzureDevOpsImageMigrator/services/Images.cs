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

public record WorkItemProperties(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("fields")] Fields Fields,
    [property: JsonPropertyName("_links")] Links Links);

public record Links([property: JsonPropertyName("workItemComments")]
    WorkItemComments WorkItemComments);

public record WorkItemComments([property: JsonPropertyName("href")] string Href);

public record Fields(
    [property: JsonPropertyName("Microsoft.VSTS.Common.AcceptanceCriteria")]
    string AcceptanceCriteria,
    [property: JsonPropertyName("System.Description")]
    string Description
);

public record CommentResult(
    [property: JsonPropertyName("comments")]
    List<Comments> Comments
);

public record Comments(
    [property: JsonPropertyName("workItemId")]
    int WorkItemId,
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("text")] string Text
);

public record ImageStream(int Id, Stream Image, string FileName, string FileId);

public record Images(int OldId, string Url);