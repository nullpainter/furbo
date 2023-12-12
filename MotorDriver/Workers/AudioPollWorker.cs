using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MotorDriver.Workers;

/// <summary>
///     Polls an audio device, randomly driving the Furby's motor when audio is playing.
/// </summary>
public class AudioPollWorker(ILogger<AudioPollWorker> logger, MotorSequence sequence) : BackgroundService
{
   
    private readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    ///     Path to audio device containing running state.
    /// </summary>
    private const string AudioDevice = "/proc/asound/card1/pcm0p/sub0/status";

    /// <summary>
    ///     Text in audio device which indicates that audio is being played.
    /// </summary>
    private const string RunningText = "state: RUNNING";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Listening...");
        
        var timer = new PeriodicTimer(_pollInterval);

        try
        {
            do
            {
                using var audioDeviceReader = new StreamReader(AudioDevice);
                var contents = await audioDeviceReader.ReadLineAsync(stoppingToken);

                // Start motor if audio is playing
                if (contents == RunningText)
                {
                    sequence.Start();
                    continue;
                }

                // Stop motor if no audio is playing
                sequence.Stop();
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            sequence.Stop();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        sequence.Stop();
    }
}