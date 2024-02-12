using System.Text.Json;

namespace AzureDevOpsImageMigrator.services;

internal class FileReader
{
    internal static T? ReadFile<T>(string path) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(path));

    internal static AppSettings ReadAppSettings(string path) =>
        ReadFile<AppSettings>(path) ?? throw new Exception("AppSettings not found");
}

public record AppSettings(string FromUrl, string FromProject, string FromUser, string FromPat, string ToUrl,
    string ToProject, string ToUser, string ToPat);