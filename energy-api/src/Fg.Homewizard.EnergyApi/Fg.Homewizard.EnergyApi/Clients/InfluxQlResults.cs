namespace Fg.Homewizard.EnergyApi.Clients
{
    public class InfluxQlResults
    {
        public IEnumerable<InfluxQlResult> Results { get; set; }

        /// <summary>
        /// Returns true if this <see cref="InfluxQlResults"/> object contains data; otherwise false.
        /// </summary>
        public bool ContainsData
        {
            get
            {
                return Results != null && Results.Any(r => r.ContainsData);
            }
        }
    }

    public class InfluxQlResult
    {
        public IEnumerable<InfluxQlSerie> Series { get; set; }

        /// <summary>
        /// Returns true if this <see cref="InfluxQlResult"/> object contains data, otherwise false.
        /// </summary>
        public bool ContainsData
        {
            get
            {
                return Series != null && Series.Any(s => s.Values != null && s.Values.Any());
            }
        }
    }

    public class InfluxQlSerie
    {
        public string Name { get; set; }
        public IEnumerable<string> Columns { get; set; }
        public IEnumerable<IEnumerable<object>> Values { get; set; }
    }
}
