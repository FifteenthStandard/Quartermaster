# Quartermaster

Search and download torrents. Torrents are sourced from The Pirate Bay via the
API at `https://apibay.org/q.php?q=%SEARCH_TERMS%`.

## Usage

```plaintext
qm search <terms>        Search for torrents
Arguments:
  <terms>  Search terms, e.g. title, IMDb ID

qm download <hash>       Download a torrent
Arguments:
  <hash>  Torrent info hash

qm config <key>          Get configuration
Arguments:
  <key>  The config setting to get
         Possible values:
         - directory  The directory to which torrents are downloaded

qm config <key> <value>  Set configuration
Arguments:
  <key>   The config setting to set
           Possible values:
           - directory  The directory to which torrents are downloaded
  <value>  The new value to set
```

## Example Usage

```bash
# Search for a torrent by title
$ qm search the godfather
Hash                                        Size        Seeds    Name
----------------------------------------    --------    -----    --------------------------------------------------------------------------------
0123456789ABCDEF0123456789ABCDEF01234567       2.4GB      256    The Godfather (1972)
...

# Search for a torrent by IMDb ID
$ qm search tt0068646
Hash                                        Size        Seeds    Name
----------------------------------------    --------    -----    --------------------------------------------------------------------------------
0123456789ABCDEF0123456789ABCDEF01234567       2.4GB      256    The Godfather (1972)
...

# Download a torrent to current directory
$ qm download 0123456789ABCDEF0123456789ABCDEF01234567
Downloading torrent file to ~/0123456789ABCDEF0123456789ABCDEF01234567.torrent
Downloading to ~/The Godfather (1972)
 15%    Downloading
 15%    The.Godfather.1972.mp4
 85%    The.Godfather.1972.srt

# Set download directory with environment variable
$ export QM_DIRECTORY=~/torrents
$ qm download 0123456789ABCDEF0123456789ABCDEF01234567
Downloading torrent file to ~/torrents/0123456789ABCDEF0123456789ABCDEF01234567.torrent
Downloading to ~/torrents/The Godfather (1972)
 15%    Downloading
 15%    The.Godfather.1972.mp4
 85%    The.Godfather.1972.srt

# Set download directory via config file
$ unset QM_DIRECTORY
$ qm config directory ~/torrents

# Confirm configuration
$ qm config directory
~/torrents

# Manually confirm configuration
$ cat ~/.qmconfig
directory = ~/torrents
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
dotnet run -- <command> [<arguments>]
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