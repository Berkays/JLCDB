using System.Net.Http.Headers;

internal static class Fetcher
{
    const string url = "https://jlcpcb.com/componentSearch/uploadComponentInfo";
    public static async Task<string> GetRemoteFilename()
    {
        HttpClient client = new();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/csv"));

        var headerRequest = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        string contentDisposition = headerRequest.Content.Headers.GetValues("Content-Disposition").First();
        int idx = contentDisposition.IndexOf("=");
        string filename = contentDisposition[(idx + 1)..contentDisposition.Length];

        return filename;
    }

    public static async Task<string> Download()
    {
        HttpClient client = new();

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/csv"));

        var downloadStream = await client.GetStreamAsync(url);

        string outFilePath = Path.Combine(Path.GetTempPath(), "input.csv");

        using var sw = new FileStream(outFilePath, FileMode.CreateNew);
        await downloadStream.CopyToAsync(sw);

        return outFilePath;
    }
}