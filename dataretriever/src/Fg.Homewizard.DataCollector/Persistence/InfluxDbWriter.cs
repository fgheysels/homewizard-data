using Fg.HomeWizard.EnergyApi.Client;
using Microsoft.Extensions.Logging;

namespace Fg.Homewizard.DataCollector.Persistence
{
    internal class InfluxDbWriter : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _databaseName;
        private readonly ILogger<InfluxDbWriter> _logger;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfluxDbWriter"/> class.
        /// </summary>
        public InfluxDbWriter(string influxDbAddress, string databaseName, ILogger<InfluxDbWriter> logger)
        {
            if (String.IsNullOrWhiteSpace(influxDbAddress))
            {
                throw new ArgumentException("The address of InfluxDB is missing", nameof(influxDbAddress));
            }

            if (String.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException("The databasename is not specified", nameof(databaseName));
            }

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(influxDbAddress);

            _databaseName = databaseName;

            _logger = logger;
        }

        public async Task<InfluxDbWriteResult> StoreElectricityMeasurementAsync(Measurement measurement)
        {
            var fieldValueMap = new (string, Func<Measurement, double>)[]
            {
                ("totalpower_import_kwh", m => m.TotalPowerImportInKwh),
                ("totalpower_export_kwh", m => m.TotalPowerExportInKwh)
            };

            return await StoreMeasurementAsync("electricity", measurement, fieldValueMap);
        }

        public async Task<InfluxDbWriteResult> StoreGasMeasurementAsync(Measurement measurement)
        {
            var fieldValueMap = new (string, Func<Measurement, double>)[]
            {
                ("totalgas_m3", m=>m.TotalGasInM3)
            };

            return await StoreMeasurementAsync("gas", measurement, fieldValueMap);
        }

        private async Task<InfluxDbWriteResult> StoreMeasurementAsync(string seriesName, Measurement measurement, IEnumerable<(string fieldName, Func<Measurement, double> measurementSelector)> fieldValueMap)
        {
            await _lock.WaitAsync();
            try
            {
                HttpRequestMessage writeRequest =
                    new HttpRequestMessage(HttpMethod.Post, $"/write?db={_databaseName}&precision=s");

                string lineProtocolMessage = ConvertMeasurementToLineProtocol(seriesName, measurement, fieldValueMap);

                writeRequest.Content = new StringContent(lineProtocolMessage);

                var response = await _httpClient.SendAsync(writeRequest);

                if (response.IsSuccessStatusCode == false)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogError(
                        $"Failed to write measurements contents to InfluxDb on {_httpClient.BaseAddress.ToString()}:");
                    _logger.LogError($"Reason: {responseContent}");
                    _logger.LogDebug(lineProtocolMessage);

                    return new InfluxDbWriteResult(false, responseContent);
                }

                return new InfluxDbWriteResult(true, string.Empty);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while writing to InfluxDb");
                return new InfluxDbWriteResult(false, exception.Message);
            }
            finally
            {
                _lock.Release();
            }
        }

        private static string ConvertMeasurementToLineProtocol(string measurementName, Measurement measurement, IEnumerable<(string fieldName, Func<Measurement, double> measurementSelector)> fieldValueMap)
        {
            if (fieldValueMap.Any() == false)
            {
                throw new ArgumentException("At least one fieldValue-mapping must be specified.", nameof(fieldValueMap));
            }

            string lineProtocol = $"{measurementName},homewizard_device={measurement.HomewizardDeviceId} ";

            foreach (var map in fieldValueMap)
            {
                lineProtocol += FormattableString.Invariant($"{map.fieldName}={map.measurementSelector(measurement)},");
            }

            lineProtocol = lineProtocol.Substring(0, lineProtocol.Length - 1);

            lineProtocol += FormattableString.Invariant($" {measurement.Timestamp.ToUnixTimeSeconds()}");

            return lineProtocol;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _lock.Wait();
            try
            {
                _httpClient?.Dispose();
                IsDisposed = true;
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    public class InfluxDbWriteResult
    {
        public bool Success { get; }

        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfluxDbWriteResult"/> class.
        /// </summary>
        public InfluxDbWriteResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
