using MonoTorrent;
using MonoTorrent.Client;

public class DownloadCommand : Command
{
    public DownloadCommand()
        : base("download", "Download a torrent")
    {
        var hash = new Argument<string>("hash", "Torrent info hash");

        this.Add(hash);

        this.SetHandler(async hash =>
        {
            var downloadDirectory = Config.DownloadDirectory ?? Directory.GetCurrentDirectory();

            Directory.CreateDirectory(downloadDirectory);

            if (!await DownloadTorrentFileAsync(downloadDirectory, hash)) return;
            if (!await DownloadTorrentAsync(downloadDirectory, hash)) return;
        }, hash);
    }

    private async Task<bool> DownloadTorrentFileAsync(string downloadDirectory, string hash)
    {
        var torrentFilename = $"{hash}.torrent";

        var torrentFile = Path.Join(downloadDirectory, torrentFilename);

        if (File.Exists(torrentFile)) return true;

        Console.WriteLine($"Downloading torrent file to {torrentFile}");

        byte[] bytes;
        using (var client = new HttpClient())
        {
            var url = $"https://itorrents.org/torrent/{torrentFilename}";
            try
            {
                bytes = await client.GetByteArrayAsync(url);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error downloading torrent file: {ex.Message}");
                return false;
            }
        }

        try
        {
            await File.WriteAllBytesAsync(torrentFile, bytes);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving torrent file: {ex.Message}");
            return false;
        }

        return true;
    }

    private async Task<bool> DownloadTorrentAsync(string downloadDirectory, string hash)
    {
        using (var engine = new ClientEngine())
        {
            Torrent torrent;
            try
            {
                torrent = await Torrent.LoadAsync(
                    Path.Join(downloadDirectory, $"{hash}.torrent"));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error reading torrent file: {ex.Message}");
                return false;
            }

            var manager = await engine.AddAsync(
                torrent,
                downloadDirectory);

            try
            {
                await manager.StartAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error starting download: {ex.Message}");
                return false;
            }

            while (manager.State != TorrentState.Seeding)
            {
                Console.CursorVisible = false;
                Console.WriteLine($"Downloading to {manager.ContainingDirectory}");

                if (manager.State == TorrentState.Error)
                {
                    Console.Error.WriteLine($"Error downloading torrent: {manager.Error.Reason} {manager.Error.Exception.Message}");
                    return false;
                }
                Console.WriteLine($"{manager.PartialProgress,3:f0}%    {manager.State,-16}");
                foreach (var file in manager.Files)
                {
                    Console.WriteLine($"{Math.Floor(100.00 * file.BytesDownloaded() / file.Length),3:f0}%    {file.Path}");
                }
                await Task.Delay(1000);
            }

            try
            {
                await manager.StopAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error stopping download: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}