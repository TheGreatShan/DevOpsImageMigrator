﻿using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AzureDevOpsImageMigrator.services;

namespace AzureDevOpsImageMigrator;

public class Program
{
    public static void Main(string[] args)
    {
        var appSettings = FileReader.ReadAppSettings("appsettings.json");
        var encodedPat = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{appSettings.FromPat}"));
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedPat);


        var query = new
        {
            query = "Select [System.Id], [System.Title], [System.State] From WorkItems"
        };

        var content = JsonSerializer.Serialize(query);
        var result = client.PostAsync($"{appSettings.FromUrl}_apis/wit/wiql?api-version=6.0", new StringContent(content, Encoding.UTF8, "application/json")).Result;

        Console.WriteLine(result.Content.ReadAsStringAsync().Result);
    }
}