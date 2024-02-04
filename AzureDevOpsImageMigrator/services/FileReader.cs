using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureDevOpsImageMigrator.services;

internal class FileReader
{
    internal static T? ReadFile<T>(string path) => 
        JsonSerializer.Deserialize<T>(path);
    
    internal static AppSettings ReadAppSettings(string path) => 
        ReadFile<AppSettings>(path) ?? throw new Exception("AppSettings not found");
}

internal record AppSettings(string FromUrl, string FromUser, string FromPat, string ToUrl, string ToUser, string ToPat);