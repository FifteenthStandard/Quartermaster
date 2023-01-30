# Quartermaster

Search and download torrents. Torrents are sourced from The Pirate Bay via the
API at `https://apibay.org/q.php?q=%SEARCH_TERMS%`.

## Usage

```plaintext
qm <search>... [options]

Arguments:
  <search>  Search terms, e.g. title, IMDb ID

Options:
  -p, --path <path>  Download directory [default: current working directory]
```

## Dependencies

Quartermaster uses the following dependencies:

* Developed with [.NET 7]
* CLI developed with [System.CommandLine]
* `https://apibay.org/q.php?q=%SEARCH_TERMS%` to search for torrents
* `https://itorrents.org/torrent/%INFO_HASH%.torrent` to download torrent files
* [MonoTorrent] to download torrents

## Build

To build, download and install the .NET 7 SDK and then run the following.

```bash
dotnet build
```

To run locally, run the following.

```bash
dotnet run -- <search>... [options]
```

To build a standalone executable, run the following.

```bash
dotnet publish
```

This standalone executable can then be copied to a directory on your `PATH` to
easily run from anywhere.

[.NET 7]: https://dotnet.microsoft.com/en-us/download/dotnet/7.0
[MonoTorrent]: https://www.nuget.org/packages/MonoTorrent/
[System.CommandLine]: https://www.nuget.org/packages/System.CommandLine