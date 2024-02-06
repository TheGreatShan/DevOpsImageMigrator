using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureDevOpsImageMigrator.services;

internal class FileReader
{
    internal static T? ReadFile<T>(string path) => 
        JsonSerializer.Deserialize<T>(File.ReadAllText(path));
    
    internal static AppSettings ReadAppSettings(string path) => 
        ReadFile<AppSettings>(path) ?? throw new Exception("AppSettings not found");
}

public record AppSettings(string FromUrl, string FromUser, string FromPat, string ToUrl, string ToUser, string ToPat);