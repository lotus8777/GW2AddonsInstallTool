using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GW2_addons_installtool.Services;

public class Downlodmeg
{
    private static readonly HttpClient SharedHttpClient = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        HttpClientHandler handler = new HttpClientHandler
        {
            AllowAutoRedirect = true
        };
        HttpClient client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        client.Timeout = TimeSpan.FromMinutes(10);
        return client;
    }

    public static async Task<bool> DownloadFileAsync(string url, string destPath, Action<long, long> progressCallback)
    {
        try
        {
            Logger.Log($"开始下载文件: {Path.GetFileName(destPath)} | URL: {url}");

            string? dir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using HttpResponseMessage response = await SharedHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;

            using FileStream fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            using Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            byte[] buffer = new byte[81920];
            long totalRead = 0L;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                totalRead += bytesRead;
                progressCallback?.Invoke(totalRead, totalBytes);
            }

            Logger.Log($"文件下载成功: {Path.GetFileName(destPath)} (共 {totalRead} 字节)");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"下载失败 [{Path.GetFileName(destPath)}] ({url})", ex);
            return false;
        }
    }
}
