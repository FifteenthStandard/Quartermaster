public class QuartermasterCommand : RootCommand
{
    public QuartermasterCommand()
        : base("Search and download torrents")
    {
        var search = new Argument<string[]>("search", "Search terms, e.g. title, IMDb ID")
        {
            Arity = ArgumentArity.OneOrMore
        };
        var downloadDirectory = new Option<string>("--path", () => Environment.CurrentDirectory, "Download directory");
        downloadDirectory.AddAlias("-p");

        this.Add(search);
        this.Add(downloadDirectory);

        this.SetHandler(async (search, downloadDirectory) =>
        {
            var consoleManager = new ConsoleManager();
            var searchManager = new SearchManager();
            var torrentFileManager = new TorrentFileManager(downloadDirectory);
            var torrentDownloadManager = new TorrentDownloadManager(downloadDirectory);

            try
            {
                var searchString = string.Join(' ', search);
                var results = await searchManager.SearchAsync(searchString);
                var infoHash = consoleManager.SelectTorrent(results);

                var torrentFile = await torrentFileManager.DownloadAsync(infoHash);

                await torrentDownloadManager.LoadAsync(torrentFile);
                await consoleManager.SelectFilesAsync(torrentDownloadManager);
                await torrentDownloadManager.StartAsync();
                await consoleManager.MonitorProgressAsync(torrentDownloadManager);
                await torrentDownloadManager.StopAsync();

                torrentFileManager.Delete(infoHash);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }, search, downloadDirectory);
    }
}