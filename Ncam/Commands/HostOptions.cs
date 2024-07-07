using System.CommandLine;

namespace Ncam.Commands
{
    public static class HostOptions
    {
        public static readonly Option<List<string>> Hostname = new(["--hostname", "-h"], "The host name.");
        public static readonly Option<List<string>> RecordType = new(["--recordType", "-r"], "The record type.");
        public static readonly Option<List<string>> Address = new(["--address", "-a"], "The address.");
    }
}
