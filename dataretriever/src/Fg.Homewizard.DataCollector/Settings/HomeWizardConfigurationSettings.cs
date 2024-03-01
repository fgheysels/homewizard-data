using Microsoft.Extensions.Configuration;

namespace Fg.Homewizard.DataCollector.Settings
{
    public class HomeWizardConfigurationSettings
    {
        /// <summary>
        /// The hostname of the HomeWizard P1 device.
        /// </summary>
        [ConfigurationKeyName("P1HostName")]
        public string P1HostName { get; set; }
    }
}
