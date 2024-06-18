using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace NamecheapAutomation
{
    public class Query
    {
        private readonly GlobalParameters _globals;
        private readonly Dictionary<string, string> _parameters = [];

        public Query(GlobalParameters globals)
        {
            ArgumentNullException.ThrowIfNull(globals);

            _globals = globals;
        }

        public Query SetParameter(string key, string value)
        {
            if (!_parameters.TryAdd(key, value))
            {
                _parameters[key] = value;
            }

            return this;
        }

        public async Task<XDocument> ExecuteAsync(string command)
        {
            var baseUrl = _globals.IsSandBox ? "https://api.sandbox.namecheap.com/xml.response" : "https://api.namecheap.com/xml.response";
            var param = new Dictionary<string, string?>
            {
                { "UserName", _globals.UserName },
                { "ApiUser", _globals.UserName },
                { "ApiKey", _globals.ApiKey },
                { "ClientIp", _globals.ClientIp },
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

            return XDocument.Parse(xml);
        }
    }
}
