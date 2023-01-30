using MonoTorrent;
using MonoTorrent.Client;

public class TorrentDownloadManager
{
    private readonly string _downloadDirectory;
    private readonly ClientEngine _engine;
    private string? _torrentFile;
    private TorrentManager? _manager;

    public TorrentDownloadManager(string downloadDirectory)
    {
        _downloadDirectory = downloadDirectory;
        _engine = new ClientEngine();
    }

    public async Task LoadAsync(string torrentFile)
    {
        _torrentFile = torrentFile;

        Torrent torrent;
        try
        {
            torrent = await Torrent.LoadAsync(torrentFile);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading torrent file: {ex.Message}");
        }

        _manager = await _engine.AddAsync(
            torrent,
            _downloadDirectory);
    }

    public IList<(string Path, long Size, bool Download)> Files
        => _manager!.Files
            .Select(file => (file.Path, file.Length, file.Priority == Priority.Normal))
            .ToList();
    
    public Task SetDownloadAsync(int fileIndex, bool download)
        => _manager!.SetFilePriorityAsync(
            _manager.Files[fileIndex],
            download ? Priority.Normal : Priority.DoNotDownload);

    public Task StartAsync() => _manager!.StartAsync();

    public (string Name, string Directory) TorrentInfo => (_manager!.Torrent.Name, _manager!.ContainingDirectory);

    public (double Progress, int Seeds, IEnumerable<(string Path, double Progress)> Files) GetProgress()
        => _manager!.State switch
        {
            TorrentState.Error => throw new Exception($"Error downloading torrent: {_manager!.Error.Reason}"),
            TorrentState.Seeding => (
                100.00,
                0,
                _manager!.Files
                    .Where(file => file.Priority != Priority.DoNotDownload)
                    .Select(file => (file.Path, 100.00))),
            _ => (
                _manager!.PartialProgress,
                _manager!.Peers.Seeds,
                _manager!.Files
                    .Where(file => file.Priority != Priority.DoNotDownload)
                    .Where(file => file.BytesDownloaded() < file.Length)
                    .Select(file => (file.Path, 100.00 * file.BytesDownloaded() / file.Length)))
        };

    public async Task StopAsync()
    {
        await _manager!.StopAsync();

        foreach (var file in _manager!.Files.Where(file => file.Priority == Priority.DoNotDownload))
        {
            if (File.Exists(file.FullPath)) File.Delete(file.FullPath);
        }

        DeleteEmptySubdirectories(_manager!.ContainingDirectory);

        void DeleteEmptySubdirectories(string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteEmptySubdirectories(directory);
                if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory);
            }
        }
    }
}