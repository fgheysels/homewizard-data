using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Globalization;
using System.Text;

namespace Fg.Homewizard.EnergyApi.Infra
{
    public class CsvFormatter : TextOutputFormatter
    {
        public CsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/csv"));

            SupportedEncodings.Add(new UTF8Encoding(false));
            SupportedEncodings.Add(Encoding.ASCII);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            StringBuilder csv = new StringBuilder();

            Type type = GetTypeOf(context.Object);
            csv.AppendLine(string.Join<string>(ListSeparator, type.GetProperties().Select(x => x.Name)));

            foreach (var obj in (IEnumerable<object>)context.Object)
            {
                var propertyValues = obj.GetType().GetProperties().Select(
                    pi => new
                    {
                        Value = pi.GetValue(obj, null)
                    }
                );

                List<string> recordValues = new List<string>();

                foreach (var val in propertyValues)
                {
                    if (val.Value != null)
                    {
                        var tmpval = val.Value.ToString();

                        //Check if the value contans a comma and place it in quotes if so
                        if (tmpval.Contains(ListSeparator))
                        {
                            tmpval = string.Concat("\"", tmpval, "\"");
                        }

                        tmpval = tmpval.Replace("\r", " ", StringComparison.InvariantCultureIgnoreCase);
                        tmpval = tmpval.Replace("\n", " ", StringComparison.InvariantCultureIgnoreCase);

                        recordValues.Add(tmpval);
                    }
                    else
                    {
                        recordValues.Add(string.Empty);
                    }
                }
                csv.AppendLine(string.Join(ListSeparator, recordValues));
            }
            return context.HttpContext.Response.WriteAsync(csv.ToString(), selectedEncoding);
        }

        private static Type GetTypeOf(object obj)
        {
            Type type = obj.GetType();
            Type itemType;
            if (type.GetGenericArguments().Length > 0)
            {
                itemType = type.GetGenericArguments()[0];
            }
            else
            {
                itemType = type.GetElementType();
            }
            return itemType;
        }

        private static readonly string ListSeparator =  CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        //private void WriteHeader(StreamWriter writer)
        //{
        //    writer.WriteLine($"Timestamp{ListSeparator}PowerImportReading{ListSeparator}PowerExportReading{ListSeparator}PowerImport{ListSeparator}PowerExport");
        //}

        //private void WriteItem(PowerUsage powerUsage, StreamWriter writer)
        //{
        //    writer.WriteLine(
        //        $"{powerUsage.Timestamp}{ListSeparator}{powerUsage.PowerImportReading}{ListSeparator}{powerUsage.PowerExportReading}{ListSeparator}{powerUsage.PowerImport}{ListSeparator}{powerUsage.PowerExport}");
        //}
    }
}
