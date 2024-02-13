using System.Text;
using AzureDevOpsImageMigrator.services;
using Serilog;

namespace AzureDevOpsImageMigrator;

public class Program
{
    public static void Main(string[] args)
    {
        using var logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        var appSettings = FileReader.ReadAppSettings("appsettings.json");
        logger.Information("AppSettings: Project: {project}; User: {user}", appSettings.FromProject,
            appSettings.FromUser);

        var encodedPat = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{appSettings.FromPat}"));
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", encodedPat);

        try
        {
            client
                .GetWorkItems(appSettings, logger)
                .GetImages(client)
                .GetImageStream(client, logger)
                .GetImageStream(logger)
                .SaveImage(logger);
        }
        catch (Exception e)
        {
            logger.Error(e, "Error occurred");
        }
        finally
        {
            Console.ReadKey();
        }
    }
}