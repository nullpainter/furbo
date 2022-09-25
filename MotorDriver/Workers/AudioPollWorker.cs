using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MotorDriver.Models;

namespace MotorDriver.Workers;

/// <summary>
///     Polls an audio device, randomly driving the Furby's motor when audio is playing.
/// </summary>
public class AudioPollWorker : IHostedService
{
    /// <summary>
    ///     Range of motor durations.
    /// </summary>
    private readonly Range _durations = new(20, 100);

    private readonly TimeSpan _pollInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    ///     Path to audio device containing running state.
    /// </summary>
    private const string AudioDevice = "/proc/asound/card1/pcm0p/sub0/status";

    /// <summary>
    ///     Text in audio device which indicates that audio is being played.
    /// </summary>
    private const string RunningText = "state: RUNNING";

    private MotorDriver _driver;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _driver = new MotorDriver();
        var timer = new PeriodicTimer(_pollInterval);

        try
        {
            do
            {
                using var audioDeviceReader = new StreamReader(AudioDevice);
                var contents = await audioDeviceReader.ReadLineAsync();

                // Start motor if audio is playing
                if (contents == RunningText)
                {
                    _driver.Start(NewState);
                    continue;
                }

                // Stop motor if no audio is playing
                _driver.Stop();
            } while (await timer.WaitForNextTickAsync(cancellationToken));
        }
        catch (OperationCanceledException)
        {
            _driver.Stop();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _driver.Stop();
        return Task.CompletedTask;
    }

    // Creates new duration and direction state for the motor driver
    private State NewState()
    {
        var direction = Random.Shared.Next() % 2 == 0 ? Direction.Forward : Direction.Backward;
        var duration = Random.Shared.Next(_durations.Start.Value, _durations.End.Value);

        return new State(direction, duration);
    }
}