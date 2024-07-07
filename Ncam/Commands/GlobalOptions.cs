using System.CommandLine;

namespace Ncam.Commands
{
    public static class GlobalOptions
    {
        internal static readonly Option<string> Domain = new(["--domain", "-d"], "The domain to manage DNS records on.")
        {
            IsRequired = true
        };

        internal static readonly Option<string> Username = new(["--username", "-u"], "The Namecheap username.")
        {
            IsRequired = true
        };

        internal static readonly Option<string> ApiKey = new(["--apiKey", "-k"], "The Namecheap API key.")
        {
            IsRequired = true
        };

        internal static readonly Option<bool> Sandbox = new(["--sandbox", "-s"], "Use the Namecheap sandbox API.");
    }
}
