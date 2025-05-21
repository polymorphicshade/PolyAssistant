using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#pragma warning disable CS1998

namespace PolyAssistant.Cli.Services;

public sealed class MainService(ILogger<MainService> logger, IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // from command-line args
        var value1 = configuration["param1"];
        var value2 = configuration["param2"];

        // TODO: fill in
        //
        //

        // all done
        hostApplicationLifetime.StopApplication();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // TODO: fill in
        //
        //
    }
}