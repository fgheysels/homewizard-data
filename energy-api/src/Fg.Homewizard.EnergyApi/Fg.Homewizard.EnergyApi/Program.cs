
using Fg.Homewizard.EnergyApi.Clients;
using Fg.Homewizard.EnergyApi.Configuration;
using Fg.Homewizard.EnergyApi.Infra;
using Fg.Homewizard.EnergyApi.Services;

namespace Fg.Homewizard.EnergyApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<InfluxDbSettings>(builder.Configuration.GetSection("InfluxDb"));
            
            builder.Services.AddHttpClient<InfluxDbReader>();
            builder.Services.AddScoped<InfluxDbReader>();
            builder.Services.AddScoped<EnergyConsumptionRetriever>();

            builder.Services.AddRouting(options => { options.LowercaseUrls = true; });
            builder.Services.AddControllers(options => options.OutputFormatters.Add(new CsvFormatter()));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            var app = builder.Build();
            
            // Configure the HTTP request pipeline.

            app.UseSwagger(swaggerOptions =>
            {
                swaggerOptions.RouteTemplate = "api/{documentName}/docs.json";
            });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api/v1/docs.json", "Homewizard Energy");
                options.RoutePrefix = "api/docs";
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
