using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlexaInvoker.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Pv;

namespace AlexaInvoker.Workers;

/// <summary>
///     Listens for a wake word, invoking Alexa.
/// </summary>
public class Invoker(IOptions<Configuration> options) : BackgroundService
{
    private readonly Porcupine _porcupine = Porcupine.FromKeywordPaths(options.Value.AccessKey, new[] { options.Value.KeywordPath });

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Start Alexa application in a separate terminal
        InvokeTmux("new", "-d");
        InvokeTmux("send-keys", options.Value.AlexaPath, "Enter");
        
        using var recorder = PvRecorder.Create(deviceIndex: -1, frameLength: _porcupine.FrameLength);
        Console.WriteLine("Listening...");
        
        recorder.Start();

        // Listen for wake words
        while (!stoppingToken.IsCancellationRequested)
        {
            var pcm = recorder.Read();
            
            var result = _porcupine.Process(pcm);
            if (result >= 0)
            {
                Console.WriteLine("Invoking Alexa");
                InvokeTmux("send-keys", "t", "Enter"); 
            }

            Thread.Yield();
        }

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        
        _porcupine.Dispose();
        InvokeTmux("kill-session");
    }

    private static void InvokeTmux(params string[] arguments) => Process.Start("tmux", arguments.Append("-sfurbo"));
}