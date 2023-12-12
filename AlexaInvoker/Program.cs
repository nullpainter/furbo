using AlexaInvoker.Models;
using AlexaInvoker.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureLogging(c => c.ClearProviders())
    .ConfigureServices((context, services) =>
    {
        var configSection = context.Configuration.GetSection(Configuration.ConfigurationKey);

        services
            .AddOptions<Configuration>()
            .Bind(configSection)
            .ValidateDataAnnotations();

        services.AddHostedService<Invoker>();
    })
    .Build()
    .RunAsync();