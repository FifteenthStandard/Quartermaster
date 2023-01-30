public class TorrentFileManager
{
    private readonly string _downloadDirectory;

    public TorrentFileManager(string downloadDirectory)
    {
        _downloadDirectory = downloadDirectory;
    }

    public async Task<string> DownloadAsync(string infoHash)
    {
        var torrentFilename = $"{infoHash}.torrent";

        var torrentFile = Path.Join(_downloadDirectory, torrentFilename);

        if (File.Exists(torrentFile)) return torrentFile;

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
                throw new Exception($"Error downloading torrent file: {ex.Message}");
            }
        }

        try
        {
            await File.WriteAllBytesAsync(torrentFile, bytes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving torrent file: {ex.Message}");
        }

        return torrentFile;
    }

    public void Delete(string infoHash)
    {
        var torrentFilename = $"{infoHash}.torrent";
        if (File.Exists(torrentFilename)) File.Delete(torrentFilename);
    }
}