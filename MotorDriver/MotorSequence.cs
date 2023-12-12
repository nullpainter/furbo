using System;
using MotorDriver.Models;

namespace MotorDriver;

public class MotorSequence(MotorDriver driver)
{
    /// <summary>
    ///     Range of motor durations.
    /// </summary>
    private readonly Range _durations = new(20, 100);

    public void Start() => driver.Start(NewState);

    public void Stop() => driver.Stop();

    // Creates new duration and direction state for the motor driver
    private State NewState()
    {
        var direction = Random.Shared.Next() % 2 == 0 ? Direction.Forward : Direction.Backward;
        var duration = Random.Shared.Next(_durations.Start.Value, _durations.End.Value);

        return new State(direction, duration);
    } 
}