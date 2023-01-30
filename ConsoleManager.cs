using System.Runtime.InteropServices;

public class ConsoleManager
{
    public void ClearScreen(object? _ = null, ConsoleCancelEventArgs? __ = null)
    {
        Console.CursorVisible = false;
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        Console.CursorVisible = true;
    }

    public string SelectTorrent(IEnumerable<TorrentSearchResult> results)
    {
        var resultArray = results.ToArray();

        Console.CancelKeyPress += ClearScreen;
        var cursorRow = 0;
        while (true)
        {
            Console.CursorVisible = false;
            var height = Console.WindowHeight - 1;
            var start = Math.Min(
                Math.Max(0, cursorRow - height/2),
                Math.Max(0, resultArray.Length - height));
            var end = Math.Min(start + height, resultArray.Length);
            var nameWidth = Console.WindowWidth - 16;
            for (var line = start; line < end; line++)
            {
                var result = resultArray[line];
                var highlight = line == cursorRow ? "\u001b[100m" : "";
                var lineText = $"{FormatSize(decimal.Parse(result.Size)),8} {result.Seeders,5} {Truncate(result.Name, nameWidth)}";
                if (line == cursorRow)
                {
                    var padding = new string(' ', Console.WindowWidth - lineText.Length);
                    lineText = $"\u001b[100m{lineText}{padding}\u001b[49m";
                }
                Console.WriteLine(lineText);
            }
            for (int bufferLine = resultArray.Length; bufferLine < height; bufferLine++)
            {
                Console.WriteLine();
            }
            Console.Write("[▲▼]: Move cursor    [Enter]: Confirm");
            Console.SetCursorPosition(0, cursorRow - start);
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
                        if (cursorRow + 1 < resultArray.Length) cursorRow++;
                        break;
                    case ConsoleKey.Enter:
                        ClearScreen();
                        Console.CancelKeyPress -= ClearScreen;
                        return resultArray[cursorRow].Info_Hash;
                    default:
                        continue;
                }
                break;
            }
            ClearScreen();
        }
    }

    public async Task SelectFilesAsync(TorrentDownloadManager manager)
    {
        var cursorRow = 0;
        var confirmed = false;
        var toggleAllState = true;
        Console.CancelKeyPress += ClearScreen;
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
                var selected = file.Download ? '+' : '-';
                var highlight = line == cursorRow ? "\u001b[100m" : "";
                var lineText = $"{selected} {FormatSize(file.Size),8} {file.Path}";
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
                    case ConsoleKey.Y:
                        await manager.SetDownloadAsync(
                            cursorRow,
                            true);
                        if (cursorRow + 1 < manager.Files.Count) cursorRow++;
                        break;
                    case ConsoleKey.N:
                        await manager.SetDownloadAsync(
                            cursorRow,
                            false);
                        if (cursorRow + 1 < manager.Files.Count) cursorRow++;
                        break;
                    case ConsoleKey.Spacebar:
                        var selectedFile = manager.Files[cursorRow];
                        await manager.SetDownloadAsync(
                            cursorRow,
                            !selectedFile.Download);
                        break;
                    case ConsoleKey.A:
                        toggleAllState = !toggleAllState;
                        await Task.WhenAll(
                            manager.Files.Select((file, index) =>
                                manager.SetDownloadAsync(
                                    index,
                                    toggleAllState)));
                        break;
                    case ConsoleKey.Enter:
                        confirmed = true;
                        break;
                    default:
                        continue;
                }
                break;
            }
            ClearScreen();
        }
        Console.CancelKeyPress -= ClearScreen;
        foreach (var file in manager.Files)
        {
            var selected = file.Download ? '+' : '-';
            Console.WriteLine($"{selected} {FormatSize(file.Size),8} {file.Path}");
        }
    }

    public async Task MonitorProgressAsync(TorrentDownloadManager manager)
    {
        var originalTitle = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Console.Title
            : $"{Environment.UserName}@{Environment.MachineName}";
        var (name, directory) = manager.TorrentInfo;
        var (progress, seeds, files) = manager.GetProgress();
        Console.CancelKeyPress += ClearScreen;
        while (progress < 99.999)
        {
            Console.CursorVisible = false;
            Console.Title = $"{progress,3:f0}% {name}";
            Console.WriteLine($"Downloading to {directory}");
            Console.WriteLine($"{progress,3:f0}%    Seeds: {seeds}");
            var viewableFiles = Console.WindowHeight - 3;
            var fileArray = files.Take(viewableFiles).ToArray();
            foreach (var file in fileArray)
            {
                Console.WriteLine($"{Math.Floor(file.Progress),3:f0}%    {file.Path}");
            }
            for (var ind = fileArray.Length; ind < viewableFiles; ind++)
            {
                Console.WriteLine();
            }
            await Task.Delay(1000);
            ClearScreen();
            (progress, seeds, files) = manager.GetProgress();
        }

        Console.CancelKeyPress -= ClearScreen;
        Console.Title = originalTitle;

        foreach (var file in files)
        {
            Console.WriteLine($"{Math.Floor(file.Progress),3:f0}%    {file.Path}");
        }
    }

    string Truncate(string text, int length)
        => text.Length <= length
            ? text
            : $"{text.Substring(0, length - 3)}...";

    private string FormatSize(decimal bytes)
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