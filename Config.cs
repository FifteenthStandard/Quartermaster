public static class Config
{
    private static string? _downloadDirectory;
    public static string? DownloadDirectory
    {
        get => _downloadDirectory ?? (_downloadDirectory = Get("directory"));
        set => _downloadDirectory = Set("directory", value);
    }

    private static string ConfigFile = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".qmconfig");
    private static string ConfigFileTmp = $"{ConfigFile}.tmp";

    public static string? Get(string key)
    {
        if (!File.Exists(ConfigFile)) return null;

        var prefix = $"{key} = ";

        var lines = File.ReadAllLines(ConfigFile);
        foreach (var line in lines)
        {
            if (line.StartsWith(prefix)) return line.Substring(prefix.Length);
        }

        return null;
    }

    public static string? Set(string key, string? value)
    {
        if (!File.Exists(ConfigFile)) File.WriteAllText(ConfigFile, "");

        File.WriteAllText(ConfigFileTmp, "");

        var prefix = $"{key} = ";

        bool found = false;
        var lines = File.ReadAllLines(ConfigFile);
        foreach (var line in lines)
        {
            if (line.StartsWith(prefix))
            {
                File.AppendAllLines(ConfigFileTmp, new [] { $"{prefix}{value}" });
                found = true;
            }
            else
            {
                File.AppendAllLines(ConfigFileTmp, new [] { line });
            }
        }
        if (!found) File.AppendAllLines(ConfigFileTmp, new [] { $"{prefix}{value}" });

        File.Move(ConfigFileTmp, ConfigFile, true);

        return value;
    }
}