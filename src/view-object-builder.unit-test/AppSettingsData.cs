using System.IO;
using Newtonsoft.Json;

namespace viewObjectBuilder.unitTest
{
    public class AppSettingsData
    {
        public const string DefaultAppSettingsFile = "properties/appSettings.json";

        [JsonProperty("ConnectionStringTemplate")]
        public string ConnectionStringTemplate { get; set; }

        public static AppSettingsData LoadFromFile(string fileLocation)
        {
            var data = File.ReadAllText(fileLocation);
            return JsonConvert.DeserializeObject<AppSettingsData>(data);
        }
    }
}