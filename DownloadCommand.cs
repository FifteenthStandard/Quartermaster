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
            var downloadDirectory = Environment.GetEnvironmentVariable("QM_DIRECTORY")
                ?? Config.DownloadDirectory
                ?? Directory.GetCurrentDirectory();

            Directory.CreateDirectory(downloadDirectory);

            if (!await DownloadTorrentFileAsync(downloadDirectory, hash)) return;
            if (!await DownloadTorrentAsync(downloadDirectory, hash)) return;
            if (!DeleteTorrentFile(downloadDirectory, hash)) return;
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

            while (manager.State != TorrentState.Seeding && manager.State != TorrentState.Error)
            {
                Console.CursorVisible = false;
                Console.WriteLine($"Downloading to {manager.ContainingDirectory}");
                Console.WriteLine($"{manager.PartialProgress,3:f0}%    {manager.State,-16}    {manager.Peers.Seeds,4} seeds");
                var viewableFiles = Console.WindowHeight - 3;
                var files = manager.Files
                    .Where(file => file.BytesDownloaded() < file.Length)
                    .Take(viewableFiles)
                    .ToArray();
                foreach (var file in files)
                {
                    Console.WriteLine($"{Math.Floor(100.00 * file.BytesDownloaded() / file.Length),3:f0}%    {file.Path}");
                }
                for (var ind = files.Length; ind < viewableFiles; ind++)
                {
                    Console.WriteLine();
                }
                await Task.Delay(1000);
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.CursorVisible = true;
            }

            if (manager.State == TorrentState.Error)
            {
                Console.Error.WriteLine($"Error downloading torrent: {manager.Error.Reason} {manager.Error.Exception.Message}");
                return false;
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

    private bool DeleteTorrentFile(string downloadDirectory, string hash)
    {
        var torrentFilename = $"{hash}.torrent";

        var torrentFile = Path.Join(downloadDirectory, torrentFilename);

        if (!File.Exists(torrentFile)) return true;

        Console.WriteLine($"Deleting torrent file to {torrentFile}");

        File.Delete(torrentFile);

        return true;
    }
}