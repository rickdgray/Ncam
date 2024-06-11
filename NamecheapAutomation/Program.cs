using NamecheapAutomation;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var httpClient = new HttpClient();
        var ip = await httpClient.GetStringAsync("https://api.seeip.org");

        // sandbox
        //var api = new NameCheapApi("rickdgray", "rickdgray", "", ip, true);

        // prod
        var api = new NameCheapApi("rickdgray", "rickdgray", "", ip);

        var hostEntries = new[]
        {
            new HostEntry { HostName = "@", RecordType = RecordType.ALIAS, Address = "www.rickdgray.com." },
            new HostEntry { HostName = "@", RecordType = RecordType.CAA, Address = "0 issue \"letsencrypt.org\"" },
            new HostEntry { HostName = "www", RecordType = RecordType.CNAME, Address = "rickdgray.github.io." }
        };

        await api.SetHostsAsync("rickdgray", "com", hostEntries);

        var response = await api.GetHostsAsync("rickdgray", "com");

        foreach (var host in response.HostEntries)
        {
            Console.Write(Enum.GetName(typeof(RecordType), host.RecordType) ?? string.Empty);
            Console.Write(" ");
            Console.Write(host.HostName);
            Console.Write(" ");
            Console.WriteLine(host.Address);
        }
    }
}
