using HtmlAgilityPack;

namespace AzureDevOpsImageMigrator.services;

internal static class ImageService
{
    internal static List<string> GetImageLinks(this string html)
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

    internal static (string, string) GetIdAndName(this string fileName) => 
        (fileName.Split("/").Last().Split("?").First(), fileName.Split("=").Last());
}