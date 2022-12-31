global using System.CommandLine;

var app = new RootCommand("Search and download torrents")
{
    new ConfigCommand(),
    new SearchCommand(),
    new DownloadCommand(),
};

return await app.InvokeAsync(args);