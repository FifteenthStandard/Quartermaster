using System.Net.Http.Json;
using System.Web;

public class SearchManager
{
    public async Task<IEnumerable<TorrentSearchResult>> SearchAsync(string search)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var url = $"https://apibay.org/q.php?q={HttpUtility.UrlEncode(search)}";
            return await client.GetFromJsonAsync<TorrentSearchResult[]>(url)
                ?? Enumerable.Empty<TorrentSearchResult>();
        }
    }
}
