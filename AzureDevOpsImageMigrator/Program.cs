﻿using System.Text;
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


        var imageList = client
            .GetWorkItems(appSettings)
            .GetImages(client)
            .GetImageStream(client)
            .GetImageStream(client)
            .SaveImage(client);
        Console.WriteLine(imageList);
    }
}

