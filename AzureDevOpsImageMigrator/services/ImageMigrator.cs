using System.Text;
using System.Text.Json;
using Ionic.Zip;
using Serilog;
using Serilog.Core;

namespace AzureDevOpsImageMigrator.services;

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
           
            if (workItemProperties?.Fields.SystemInfo is not null)
                workItemProperties.Fields.SystemInfo.GetImageLinks().ForEach(x => imageLinks.Add(new(workitem.Id, x)));
            
            if (workItemProperties?.Fields.ReproSteps is not null)
                workItemProperties.Fields.ReproSteps.GetImageLinks().ForEach(x => imageLinks.Add(new(workitem.Id, x)));

            if (workItemProperties?.Links.WorkItemComments.Href is not null)
            {
                var comments = JsonSerializer
                    .Deserialize<CommentResult>(client.GetAsync(workItemProperties.Links.WorkItemComments.Href)
                        .Result
                        .Content
                        .ReadAsStringAsync()
                        .Result);

                comments.Comments.ForEach(x => x.Text.GetImageLinks().ForEach(y => imageLinks.Add(new(x.WorkItemId, y))));
            }
        }

        return imageLinks;
    }

    internal static QueryResult? GetWorkItems(this HttpClient client, AppSettings appSettings, Logger logger)
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
        logger.Information("Received the following QueryResult: {queryResult}", queryResult?.Query ?? "No query result found");
        return queryResult ?? throw new Exception("No work items found");
    }


    internal static List<ImageStream> GetImageStream(this List<Images> images, HttpClient client, Logger logger)
    {
        var imageStreams = new List<ImageStream>();

        foreach (var image in images)
        {
            var bytes = client.GetAsync(image.Url).Result.Content.ReadAsByteArrayAsync().Result;
            var memoryStream = new MemoryStream(bytes);
            var stream = new ImageStream(image.OldId, memoryStream, image.Url.GetIdAndName().Item2,
                image.Url.GetIdAndName().Item1);
            
            logger.Information("Image stream got for workitem: {workItemId}", image.OldId);
            
            var any = imageStreams.Any(x => x.FileId == image.Url.GetIdAndName().Item1);
            if (any)
                continue;

            imageStreams.Add(stream);
        }

        return imageStreams;
    }

    internal static MemoryStream GetImageStream(this List<ImageStream> streams, Logger logger)
    {
        var memoryStream = new MemoryStream();
        using var zipFile = new ZipFile();
        
        streams.ForEach(x =>
        {
            var fileName = $"{x.Id}_{x.FileName}_{x.FileId}.png";
            logger.Information("File added with name: {fileName}", fileName);
            zipFile.AddEntry(fileName, x.Image);
        });
        zipFile.Save(memoryStream);
        
        
        return memoryStream;
    }

    // TODO Filepath should be adjustable with a config file
    internal static void SaveImage(this MemoryStream stream, Logger logger)
    {
        using var fileStream = new FileStream("images.zip", FileMode.Create, FileAccess.Write);
        stream.WriteTo(fileStream);
        logger.Information("Images saved to images.zip");
    }
}