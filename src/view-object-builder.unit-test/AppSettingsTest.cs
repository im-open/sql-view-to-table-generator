using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace viewObjectBuilder.unitTest
{
    public class AppSettings
    {
        private readonly AppSettingsData _appSettings;

        public AppSettings()
        {
            _appSettings = AppSettingsData.LoadFromFile(AppSettingsData.DefaultAppSettingsFile);
        }

        [Fact]
        public void settings_values_match_values()
        {
            _appSettings.ConnectionStringTemplate.ShouldBe(
                viewObjectBuilder.Properties.AppSettings.ConnectionStringTemplate);
        }

    }
}
