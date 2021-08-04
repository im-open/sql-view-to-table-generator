using System.IO;
using Microsoft.Extensions.Configuration;

namespace viewObjectBuilder.Properties
{
    public static class AppSettings
    {
        private static IConfiguration _configuration;

        private static IConfiguration configuration
            => _configuration ?? (_configuration = (new ConfigurationBuilder()
                .SetBasePath(Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Properties"))
                .AddJsonFile("appSettings.json")).Build());

        public static string ConnectionStringTemplate => configuration["ConnectionStringTemplate"];
    }
}