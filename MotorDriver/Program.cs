using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MotorDriver;
using MotorDriver.Workers;

await Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(services => services
        .AddHostedService<AudioPollWorker>()
        .AddSingleton<MotorDriver.MotorDriver>()
        .AddSingleton<MotorSequence>()
    )
    .Build()
    .RunAsync();