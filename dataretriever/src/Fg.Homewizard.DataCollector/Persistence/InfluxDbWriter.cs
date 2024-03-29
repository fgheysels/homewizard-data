﻿using Fg.HomeWizard.EnergyApi.Client;
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

        public async Task<InfluxDbWriteResult> StoreMeasurement(Measurement measurement)
        {
            await _lock.WaitAsync();
            try
            {
                HttpRequestMessage writeRequest =
                    new HttpRequestMessage(HttpMethod.Post, $"/write?db={_databaseName}&precision=s");

                string lineProtocolMessage = ConvertMeasurementToLineProtocol(measurement);
                _logger.LogInformation(lineProtocolMessage);
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

        private string ConvertMeasurementToLineProtocol(Measurement measurement)
        {
            string electricity =
                FormattableString.Invariant($"electricity,homewizard_device={measurement.HomewizardDeviceId} totalpower_import_kwh={measurement.TotalPowerImportInKwh},totalpower_export_kwh={measurement.TotalPowerExportInKwh} {measurement.Timestamp.ToUnixTimeSeconds()}");

            return electricity;
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
