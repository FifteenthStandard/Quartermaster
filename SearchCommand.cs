using System.Net.Http.Json;
using System.Web;

public class SearchCommand : Command
{
    public SearchCommand()
        : base("search", "Search for torrents")
    {
        var terms = new Argument<string[]>("terms", "Search terms, e.g. title, IMDb ID")
        {
            Arity = ArgumentArity.OneOrMore
        };

        this.Add(terms);

        this.SetHandler(async terms =>
        {
            var search = string.Join(' ', terms);

            TorrentSearchResult[]? results;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var url = $"https://apibay.org/q.php?q={HttpUtility.UrlEncode(search)}";
                try
                {
                    results = await client.GetFromJsonAsync<TorrentSearchResult[]>(url);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error searching for torrent: {ex.Message}");
                    return;
                }
            }

            if (results == null)
            {
                Console.Error.WriteLine("Unable to parse search results. Please try again.");
                return;
            }

            Console.Write($"Hash                                        ");
            Console.Write($"Size        ");
            Console.Write($"Seeds    ");
            Console.Write($"Name                                                                            ");
            Console.WriteLine();

            Console.Write($"----------------------------------------    ");
            Console.Write($"--------    ");
            Console.Write($"-----    ");
            Console.Write($"--------------------------------------------------------------------------------");
            Console.WriteLine();

            var height = Console.WindowHeight - 3;

            var initialResults = results.Take(height);

            foreach (var result in initialResults)
            {
                Console.Write($"{result.Info_Hash}    ");
                Console.Write($"{FormatSize(result.Size),8}    ");
                Console.Write($"{result.Seeders,5}    ");
                Console.Write($"{Truncate(result.Name, 80)}");
                Console.WriteLine();
            }

            var additionalResults = results.Skip(height);
            foreach (var result in additionalResults)
            {
                Console.Write("-- More  --");
                while (true)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Q:
                            Console.Write("\r");
                            return;
                        case ConsoleKey.Enter:
                            break;
                        default:
                            continue;
                    }
                    break;
                }
                Console.Write("\r");
                Console.Write($"{result.Info_Hash}    ");
                Console.Write($"{FormatSize(result.Size),8}    ");
                Console.Write($"{result.Seeders,5}    ");
                Console.Write($"{Truncate(result.Name, 80)}");
                Console.WriteLine();
            }
        }, terms);
    }

    string FormatSize(string sizeInBytes)
    {
        var bytes = decimal.Parse(sizeInBytes);
        if (bytes < 1024) return $"{bytes:f1}B";
        bytes /= 1024;
        if (bytes < 1024) return $"{bytes:f1}KB";
        bytes /= 1024;
        if (bytes < 1024) return $"{bytes:f1}MB";
        bytes /= 1024;
        if (bytes < 1024) return $"{bytes:f1}GB";
        bytes /= 1024;
        return $"{bytes:f1}TB";
    }

    string Truncate(string text, int length)
        => text.Length <= length
            ? text
            : $"{text.Substring(0, length - 3)}...";

}