using Fg.Homewizard.DataCollector.Settings;
using Fg.HomeWizard.EnergyApi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Fg.Homewizard.DataCollector.Persistence;

namespace Fg.Homewizard.DataCollector
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                Console.WriteLine("Canceling...");
                cts.Cancel();
                e.Cancel = true;
            };

            var configuration = BuildConfiguration();
            var loggerFactory = CreateLoggerFactory(configuration);

            var homeWizard = await GetHomeWizardDevice(configuration, loggerFactory);

            using var influxDbWriter = CreateInfluxDbWriter(configuration, loggerFactory);

            HomeWizardService service = new HomeWizardService(homeWizard);

            var logger = loggerFactory.CreateLogger<Program>();

            var requestIntervalInMinutes = configuration.GetValue<int>("RequestIntervalInMinutes", 60);
            
            while (cts.IsCancellationRequested == false)
            {
                await Task.Delay(DetermineWaitTime(TimeSpan.FromMinutes(requestIntervalInMinutes), logger), cts.Token);

                if (cts.IsCancellationRequested == false)
                {
                    var measurement = await service.GetCurrentMeasurementsAsync();

                    var writeElectricityResult = await influxDbWriter.StoreElectricityMeasurementAsync(measurement);

                    if (writeElectricityResult.Success == false)
                    {
                        logger.LogError("InfluxDb write for electricity failed: " + writeElectricityResult.Message);
                    }

                    var writeGasResult = await influxDbWriter.StoreGasMeasurementAsync(measurement);

                    if (writeGasResult.Success == false)
                    {
                        logger.LogError("InfluxDb write for gas failed: " + writeGasResult.Message);
                    }
                }
            }
        }

        private static TimeSpan DetermineWaitTime(TimeSpan requestInterval, ILogger logger)
        {
            DateTime currentTime = DateTime.Now;

            TimeSpan waitTime = new TimeSpan(requestInterval.Ticks - (currentTime.Ticks % requestInterval.Ticks));
            
            logger.LogInformation($"Next Measurement retrieval will happen at {currentTime.Add(waitTime).ToString("yyyy-MM-dd HH:mm:ss")}");

            return waitTime;
        }

        private static async Task<HomeWizardDevice> GetHomeWizardDevice(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            var settings = configuration.GetSection("HomeWizard").Get<HomeWizardConfigurationSettings>();

            if (String.IsNullOrWhiteSpace(settings?.P1HostName))
            {
                throw new ConfigurationErrorsException("HomeWizard P1 Meter not configured.  HomeWizard__P1HostName setting not found.");
            }

            var device = await HomeWizardDeviceResolver.FindHomeWizardDeviceAsync(settings.P1HostName, loggerFactory.CreateLogger(nameof(HomeWizardDeviceResolver)));

            if (device == null)
            {
                throw new Exception("No HomeWizard device found in network with name " + settings.P1HostName);
            }

            return device;
        }

        private static InfluxDbWriter CreateInfluxDbWriter(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            var settings = configuration.GetSection("InfluxDb").Get<InfluxDbSettings>();

            if (String.IsNullOrWhiteSpace(settings?.InfluxDbAddress))
            {
                throw new ConfigurationErrorsException("InfluxDb settings not configured.  InfluxDb__InfluxDbAddress setting not found.");
            }

            if (String.IsNullOrWhiteSpace(settings?.DatabaseName))
            {
                throw new ConfigurationErrorsException("InfluxDb settings not configured.  InfluxDb__DatabaseName setting not found.");
            }

            return new InfluxDbWriter(settings.InfluxDbAddress, settings.DatabaseName, loggerFactory.CreateLogger<InfluxDbWriter>());
        }

        private static IConfiguration BuildConfiguration()
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

            return configuration;
        }

        private static ILoggerFactory CreateLoggerFactory(IConfiguration configuration)
        {
            return LoggerFactory.Create(builder =>
            {
                // Passing in 'Configuration' in itself has no effect on the logger
                // regarding the configured log-levels.  Need to explicitly pass in
                // the Logging section.
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddSimpleConsole(options =>
                {
                    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss";
                    options.UseUtcTimestamp = false;
                });
            });
        }
    }
}
