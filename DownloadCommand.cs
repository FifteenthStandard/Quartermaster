using MonoTorrent;
using MonoTorrent.Client;

public class DownloadCommand : Command
{
    public DownloadCommand()
        : base("download", "Download a torrent")
    {
        var hash = new Argument<string>("hash", "Torrent info hash");
        var interactive = new Option<bool>("--interactive", "Interactively select files to download");

        this.Add(hash);
        this.Add(interactive);

        this.SetHandler(async (hash, interactive) =>
        {
            var downloadDirectory = Environment.GetEnvironmentVariable("QM_DIRECTORY")
                ?? Config.DownloadDirectory
                ?? Directory.GetCurrentDirectory();

            Directory.CreateDirectory(downloadDirectory);

            if (!await DownloadTorrentFileAsync(downloadDirectory, hash)) return;
            if (!await DownloadTorrentAsync(downloadDirectory, hash, interactive)) return;
            if (!DeleteTorrentFile(downloadDirectory, hash)) return;
        }, hash, interactive);
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

    private async Task<bool> DownloadTorrentAsync(string downloadDirectory, string hash, bool interactive)
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

            if (interactive) await SelectFilesAsync(manager);

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
                    .Where(file => file.Priority != Priority.DoNotDownload)
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

            foreach (var file in manager.Files.Where(file => file.Priority != Priority.DoNotDownload))
            {
                Console.WriteLine($"{Math.Floor(100.00 * file.BytesDownloaded() / file.Length),3:f0}%    {file.Path}");
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

            foreach (var file in manager.Files.Where(file => file.Priority == Priority.DoNotDownload))
            {
                if (File.Exists(file.FullPath)) File.Delete(file.FullPath);
            }

            DeleteEmptySubdirectories(manager.ContainingDirectory);

            return true;
        }
    }

    private async Task SelectFilesAsync(TorrentManager manager)
    {
        var cursorRow = 0;
        var confirmed = false;
        var toggleAllState = true;
        while (!confirmed)
        {
            Console.CursorVisible = false;
            var height = Console.WindowHeight - 1;
            var start = Math.Min(
                Math.Max(0, cursorRow - height/2),
                Math.Max(0, manager.Files.Count - height));
            var end = Math.Min(start + height, manager.Files.Count);
            for (var line = start; line < end; line++)
            {
                var file = manager.Files[line];
                var selected = file.Priority == Priority.Normal ? '+' : '-';
                var highlight = line == cursorRow ? "\u001b[100m" : "";
                var lineText = $"{selected} {FormatSize(file.Length),8} {file.Path}";
                if (line == cursorRow)
                {
                    var padding = new string(' ', Console.WindowWidth - lineText.Length);
                    lineText = $"\u001b[100m{lineText}{padding}\u001b[49m";
                }
                Console.WriteLine(lineText);
            }
            for (int bufferLine = manager.Files.Count; bufferLine < height; bufferLine++)
            {
                Console.WriteLine();
            }
            Console.Write("[▲▼]: Move cursor    [Space]: Toggle selection    [a]: Toggle all    [Enter]: Confirm");
            Console.SetCursorPosition(0, cursorRow - start);
            Console.CursorVisible = true;
            while (true)
            {
                var ch = Console.ReadKey(true);
                switch (ch.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.K:
                        if (cursorRow-1 >= 0) cursorRow--;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.J:
                        if (cursorRow + 1 < manager.Files.Count) cursorRow++;
                        break;
                    case ConsoleKey.Spacebar:
                        var selectedFile = manager.Files[cursorRow];
                        await manager.SetFilePriorityAsync(
                            selectedFile,
                            selectedFile.Priority ^ Priority.Normal);
                        break;
                    case ConsoleKey.A:
                        toggleAllState = !toggleAllState;
                        await Task.WhenAll(
                            manager.Files.Select(file =>
                                manager.SetFilePriorityAsync(
                                    file,
                                    toggleAllState ? Priority.Normal : Priority.DoNotDownload)));
                        break;
                    case ConsoleKey.Enter:
                        confirmed = true;
                        break;
                    default:
                        continue;
                }
                break;
            }
            Console.CursorVisible = false;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }
        foreach (var file in manager.Files)
        {
            var selected = file.Priority == Priority.Normal ? '+' : '-';
            Console.WriteLine($"{selected} {FormatSize(file.Length),8} {file.Path}");
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

    void DeleteEmptySubdirectories(string path)
    {
        foreach (var directory in Directory.GetDirectories(path))
        {
            DeleteEmptySubdirectories(directory);
            if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory);
        }
    }

    string FormatSize(decimal bytes)
    {
        if (bytes < 1024) return $"{bytes}B";
        bytes /= 1024;
        if (bytes < 1024) return $"{bytes:f1}KB";
        bytes /= 1024;
        if (bytes < 1024) return $"{bytes:f1}MB";
        bytes /= 1024;
        if (bytes < 1024) return $"{bytes:f1}GB";
        bytes /= 1024;
        return $"{bytes:f1}TB";
    }
}