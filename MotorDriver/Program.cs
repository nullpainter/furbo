using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MotorDriver.Workers;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddHostedService<AudioPollWorker>())
    .ConfigureLogging(c => c.ClearProviders())
    .Build()
    .RunAsync();