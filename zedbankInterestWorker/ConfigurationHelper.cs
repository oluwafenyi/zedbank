using System.IO;
using Microsoft.Extensions.Configuration;

namespace zedbankInterestWorker
{
    public static class ConfigurationManager
    {
        public static readonly IConfigurationRoot Config = _buildConfig();

        private static IConfigurationRoot _buildConfig()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.Development.json")
                .AddJsonFile($"appsettings.json");
            configuration.AddEnvironmentVariables("ZEDBANK_");
            return configuration.Build();
        }
    }
}