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


        var queryResult = client.GetWorkItems(appSettings);
        var imagesList = client.GetImages(queryResult);


        imagesList.ForEach(x => Console.WriteLine(x.OldId + "_" + x.Url));
    }
}

