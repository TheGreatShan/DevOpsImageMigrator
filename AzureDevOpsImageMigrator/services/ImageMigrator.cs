using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HtmlAgilityPack;
using Ionic.Zip;

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

public record ImageStream(int Id, Stream Image);

public record Images(int OldId, string Url);

internal static class ImageMigrator
{
    internal static List<Images> GetImages(this QueryResult? queryResult, HttpClient client)
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
                workItemProperties.Fields.Description.GetImageLinks().ForEach(x => imageLinks.Add(new(workitem.Id, x)));
        }

        return imageLinks;
    }

    internal static QueryResult? GetWorkItems(this HttpClient client, AppSettings appSettings)
    {
        var query = new
        {
            query =
                $"Select [System.Id], [System.Title], [System.State] From WorkItems WHERE [System.TeamProject] = \"{appSettings.FromProject.Replace("%20", " ")}\""
        };

        var content = JsonSerializer.Serialize(query);
        var result = client.PostAsync($"{appSettings.FromUrl}{appSettings.FromProject}/_apis/wit/wiql?api-version=6.0",
            new StringContent(content, Encoding.UTF8, "application/json")).Result;
        var queryResult = JsonSerializer.Deserialize<QueryResult>(result.Content.ReadAsStringAsync().Result);
        return queryResult ?? throw new Exception("No work items found");
    }

    private static List<string> GetImageLinks(this string html)
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

    internal static List<ImageStream> GetImageStream(this List<Images> images, HttpClient client)
    {
        var imageStreams = new List<ImageStream>();

        foreach (var image in images)
        {
            var bytes = client.GetAsync(image.Url).Result.Content.ReadAsByteArrayAsync().Result;
            var memoryStream = new MemoryStream(bytes);
            imageStreams.Add(new(image.OldId, memoryStream));
        }

        return imageStreams;
    }
    
    // TODO missing is the filename. Please check the code before and adjust the records accordingly
    internal static MemoryStream GetImageStream(this List<ImageStream> streams, HttpClient client)
    {
        var memoryStream = new MemoryStream();
        using (var zipFile = new ZipFile())
        {
            var random = new Random();
            var bytes = new Byte[10000];
            random.NextBytes(bytes);
            streams.ForEach(x => zipFile.AddEntry($"{x.Id}{random.Next()}.png", x.Image));
            zipFile.Save(memoryStream);
        }
        
        return memoryStream;
    }
    // Filepath should be adjustable with a config file
    internal static string SaveImage(this MemoryStream stream, HttpClient _)
    {
        using var fileStream = new FileStream("images.zip", FileMode.Create, FileAccess.Write);
        stream.WriteTo(fileStream);
        return "images saved to images.zip";
    }
}