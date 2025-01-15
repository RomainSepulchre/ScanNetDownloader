using System.IO;

namespace ScanNetDownloader.Logic.Helpers
{
    internal static class Exceptions
    {
        public static NotImplementedException UnknownWebDomain(string unknownUrl)
        {
            return new NotImplementedException($"Unknown scan web domain ({unknownUrl}), this domain is not compatible with ScanNetDownloader");
        }

        public static FileNotFoundException MissingSettingsJson(string jsonPath)
        {
            return new FileNotFoundException($"The settings.json file ({jsonPath}) is missing");
        }
    }
}
