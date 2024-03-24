namespace Fg.Homewizard.EnergyApi.Clients
{
    public class InfluxQlResults
    {
        public IEnumerable<InfluxQlResult> Results { get; set; }
    }

    public class InfluxQlResult
    {
        public IEnumerable<InfluxQlSerie> Series { get; set; }
    }

    public class InfluxQlSerie
    {
        public string Name { get; set; }
        public IEnumerable<string> Columns { get; set; }
        public IEnumerable<IEnumerable<object>> Values { get; set; }
    }
}
