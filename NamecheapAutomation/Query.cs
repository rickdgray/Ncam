using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace NamecheapAutomation
{
    internal class Query
    {
        private readonly XNamespace _ns = XNamespace.Get("http://api.namecheap.com/xml.response");
        private readonly GlobalParameters _globals;
        private readonly Dictionary<string, string> _parameters = [];

        internal Query(GlobalParameters globals)
        {
            ArgumentNullException.ThrowIfNull(globals);

            _globals = globals;
        }

        internal Query AddParameter(string key, string value)
        {
            _parameters.Add(key, value);
            return this;
        }

        internal async Task<XDocument> ExecuteAsync(string command)
        {
            var baseUrl = _globals.IsSandBox ? "https://api.sandbox.namecheap.com/xml.response" : "https://api.namecheap.com/xml.response";
            var param = new Dictionary<string, string?>
            {
                { "ApiUser", _globals.ApiUser },
                { "ApiKey", _globals.ApiKey },
                { "UserName", _globals.UserName },
                { "ClientIp", _globals.CLientIp },
                { "Command", command }
            };

            foreach (var parm in _parameters)
            {
                param.Add(parm.Key, parm.Value);
            }

            var requestUrl = new Uri(QueryHelpers.AddQueryString(baseUrl, param));

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(requestUrl.ToString());
            response.EnsureSuccessStatusCode();
            var xml = await response.Content.ReadAsStringAsync();

            XDocument doc = XDocument.Parse(xml);

            if (doc?.Root?.Attribute("Status")?.Value.Equals("ERROR", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                throw new Exception(string.Join(",", doc.Root.Element(_ns + "Errors").Elements(_ns + "Error").Select(o => o.Value).ToArray()));
            }
            
            return doc;
        }
    }
}
