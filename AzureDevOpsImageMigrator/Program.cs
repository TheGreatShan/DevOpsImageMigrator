using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureDevOpsImageMigrator.services;
using static AzureDevOpsImageMigrator.services.ImageMigrator;

namespace AzureDevOpsImageMigrator;

public class Program
{
    public static void Main(string[] args)
    {
        var appSettings = FileReader.ReadAppSettings("appsettings.json");
        var encodedPat = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{appSettings.FromPat}"));
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedPat);


        var query = new
        {
            query = "Select [System.Id], [System.Title], [System.State] From WorkItems"
        };

        var content = JsonSerializer.Serialize(query);
        var result = client.PostAsync($"{appSettings.FromUrl}_apis/wit/wiql?api-version=6.0",
            new StringContent(content, Encoding.UTF8, "application/json")).Result;
        var queryResult = JsonSerializer.Deserialize<QueryResult>(result.Content.ReadAsStringAsync().Result);
        // queryResult.WorkItems.ForEach(x => Console.WriteLine(x.Url));

        foreach (var workitem in queryResult.WorkItems)
        {
            var workItemProperties =
                JsonSerializer
                    .Deserialize<WorkItemProperties>(client.GetAsync(workitem.Url)
                        .Result
                        .Content
                        .ReadAsStringAsync()
                        .Result);

            if (workItemProperties?.Fields.AcceptanceCriteria is null)
                break;
            
            var imageLinks = workItemProperties.Fields.AcceptanceCriteria.GetImageLinks();

            if (imageLinks == null)
                break;
            
            imageLinks.ForEach(x => Console.WriteLine(x));
        }

    }
}

public record WorkItemProperties([property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("fields")] Fields Fields);

public record Fields([property: JsonPropertyName("Microsoft.VSTS.Common.AcceptanceCriteria")]
    string AcceptanceCriteria);