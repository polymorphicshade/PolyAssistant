using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PolyAssistant.Api.Components;
using PolyAssistant.Core;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Data.Chat;
using Serilog;

namespace PolyAssistant.Api;

public class Program
{
    public static void Main(string[] args)
    {
        const string swaggerName = "PolyAssistant.Api";
        const string swaggerDescription = "An API for various AI services.";
        const string swaggerVersion = "v1";

        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;
        var services = builder.Services;

        configuration
            .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
            .AddJsonFile("appsettings.user.json", reloadOnChange: true, optional: true)
            .AddEnvironmentVariables();

        if (configuration["Ollama:Url"]?.StartsWith("https", StringComparison.OrdinalIgnoreCase) == true)
        {
            services.AddInsecureHttpsClient();
        }
        else
        {
            services.AddHttpClient();
        }

        services
            // services
            .AddPolyAssistantCoreServicesScoped(configuration)
            // data
            .AddDbContext<ChatHistoryDbContext>((provider, options) =>
            {
                var config = provider.GetRequiredService<IOptions<CachingConfiguration>>();

                var databaseType = config.Value.DatabaseType;
                var connectionString = config.Value.ConnectionString;

                switch (databaseType)
                {
                    case DatabaseType.MySql:
                        var version = ServerVersion.AutoDetect(config.Value.ConnectionString);
                        options.UseMySql(connectionString, version);
                        break;
                    case DatabaseType.SqlServer:
                        options.UseSqlServer(connectionString);
                        break;
                    case DatabaseType.Sqlite:
                        options.UseSqlite(connectionString);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            })
            // swagger
            .AddSerilog(x => x.ReadFrom.Configuration(configuration))
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(x =>
            {
                // general info
                x.SwaggerDoc(swaggerVersion, new OpenApiInfo
                {
                    Title = swaggerName,
                    Description = swaggerDescription,
                    Version = swaggerVersion
                });

                // hide the default page ("/") from swagger
                x.AddDocumentFilterInstance(new SwaggerPathFilter(["^/$"]));

                // add generated XML docs to swagger
                var assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    var path = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");

                    if (File.Exists(path))
                    {
                        x.IncludeXmlComments(path);
                    }
                }
            })
            // other
            .AddControllers()
            .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services
            .Configure<RouteOptions>(x =>
            {
                // keep routes and queries lowercase
                x.LowercaseUrls = true;
                x.LowercaseQueryStrings = true;
            });

        services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto; });

        var app = builder.Build();

        app.UseForwardedHeaders();

        app.UseSerilogRequestLogging();

        app.UseSwagger(x =>
        {
            x.PreSerializeFilters.Add((swagger, httpReq) =>
            {
                if (httpReq.IsHttps)
                {
                    swagger.Servers = new List<OpenApiServer>
                    {
                        new() { Url = $"https://{httpReq.Host.Value}/{httpReq.Headers["X-Forwarded-Prefix"]}" }
                    };
                }
                else
                {
                    swagger.Servers = new List<OpenApiServer>
                    {
                        new() { Url = $"http://{httpReq.Host.Value}/{httpReq.Headers["X-Forwarded-Prefix"]}" }
                    };
                }

                // needed because of Nginx
                var forwardedProto = httpReq.Headers["X-Forwarded-Proto"].ToString();
                if (forwardedProto.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    swagger.Servers = new List<OpenApiServer>
                    {
                        new() { Url = $"https://{httpReq.Host.Value}/{httpReq.Headers["X-Forwarded-Prefix"]}" }
                    };
                }
            });
        });

        app.UseSwagger();

        app.UseSwaggerUI(x => { x.SwaggerEndpoint($"{swaggerVersion}/swagger.json", swaggerName); });

        app.MapSwagger();
        app.MapControllers();

        // default page
        app.MapGet("/", () =>
            Results.Content(
                """
                <p>API is running &#128077</p>
                <p>&#128214<a href="swagger">API</a></p>
                """, "text/html"));

        app.Run();
    }
}