using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace GW2_addons_installtool.Services;

public class Downlodmeg
{
    private static readonly HttpClient SharedHttpClient = new HttpClient();

    private readonly string downloadUrl;
    private readonly string update_path;
    private readonly int update_mode;

    public int Progresss;
    public bool downloadok;
    public Task taskA;

    public Downlodmeg(string dUrl, string dpath, int dmode)
    {
        downloadUrl = dUrl;
        update_path = dpath;
        update_mode = dmode;
        try
        {
            taskA = Task.Run(() => DownloadLatestRelease_http());
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, update_path + " 下载过程错误1");
            taskA = Task.CompletedTask;
        }
    }

    private async Task DownloadLatestRelease_http()
    {
        string pathhh;
        if (update_mode == 2)
        {
            pathhh = "latestRelease";
            if (Directory.Exists(pathhh))
            {
                Directory.Delete(pathhh, recursive: true);
            }
            Directory.CreateDirectory(pathhh);
        }
        else
        {
            pathhh = Path.Combine(Global.GamePath, "Installcache");
            if (!Directory.Exists(pathhh))
            {
                Directory.CreateDirectory(pathhh);
            }
        }

        try
        {
            downloadok = false;
            using HttpResponseMessage response = await SharedHttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            string fullDestPath = Path.Combine(pathhh, update_path);
            using FileStream downloadFile = File.Create(fullDestPath);
            using Stream download = await response.Content.ReadAsStreamAsync();

            byte[] buffer = new byte[81920];
            long totalBytesRead = 0L;
            while (true)
            {
                int bytesRead = await download.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    break;
                }
                await downloadFile.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                Progresss = (int)totalBytesRead;
            }
            downloadok = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, update_path + " 下载过程错误2");
        }
    }
}
