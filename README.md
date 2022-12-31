# Quartermaster

Search and download torrents

## Usage

```plaintext
qm config <key>          Get configuration
Arguments:
  <key>  The config setting to get
         Possible values:
         - download  The directory to which torrents are downloaded

qm config <key> <value>  Set configuration
Arguments:
  <key>   The config setting to set
           Possible values:
           - download  The directory to which torrents are downloaded
  <value>  The new value to set

qm search <terms>        Search for torrents
Arguments:
  <terms>  Search terms, e.g. title, IMDb ID

qm download <hash>       Download a torrent
Arguments:
  <hash>  Torrent info hash
```

## Example Usage

```bash
# Configure download directory
$ qm config download ~/torrents

# Confirm configuration
$ qm config download
~/torrents

# Manually confirm configuration
$ cat ~/.qmconfig
download = ~/torrents

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

# Download a torrent
$ qm download 0123456789ABCDEF0123456789ABCDEF01234567
Downloading torrent file to ~/torrents/0123456789ABCDEF0123456789ABCDEF01234567.torrent
Downloading to ~/torrents/The Godfather (1972)
 15%    Downloading
 15%    The.Godfather.1972.mp4
 85%    The.Godfather.1972.srt
```
