using System;
using System.Device.Gpio;
using MotorDriver.Models;

namespace MotorDriver;

public class MotorDriver : IDisposable
{
    private readonly GpioController _controller;
    private long _eventCount;

    private State _state;
    private Func<State> _stateProvider;

    private bool _running;

    public MotorDriver()
    {
        _controller = new GpioController();

        _controller.OpenPin(GpioAssignment.Encoder, PinMode.Input);
        _controller.OpenPin(GpioAssignment.Forward, PinMode.Output, PinValue.Low);
        _controller.OpenPin(GpioAssignment.Backwards, PinMode.Output, PinValue.Low);

        // Monitor for motor movement for timing control
        _controller.RegisterCallbackForPinValueChangedEvent(GpioAssignment.Encoder, PinEventTypes.Rising, OnPinEvent);
    }

    private void OnPinEvent(object sender, PinValueChangedEventArgs args)
    {
        if (_eventCount++ >= _state.Duration) ResetState();
    }

    /// <summary>
    ///     Stops the motor.
    /// </summary>
    public void Stop()
    {
        if (!_running) return;
        
        SetDirection(Direction.Stop);
        _running = false;
    }

    /// <summary>
    ///     Starts the motor.
    /// </summary>
    public void Start(Func<State> stateProvider)
    {
        if (_running) return;

        _stateProvider = stateProvider;
        ResetState();

        _running = true;
    }

    /// <summary>
    ///     Retrieves new state.
    /// </summary>
    private void ResetState()
    {
        _eventCount = 0;
        if (_stateProvider == null) return;
        
        // Retrieve new state and set direction
        _state = _stateProvider();
        SetDirection(_state.Direction);
    }

    /// <summary>
    ///     Changes the motor direction.
    /// </summary>
    private void SetDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Forward:
                _controller.Write(GpioAssignment.Backwards, PinValue.Low);
                _controller.Write(GpioAssignment.Forward, PinValue.High);
                break;
            case Direction.Backward:
                _controller.Write(GpioAssignment.Forward, PinValue.Low);
                _controller.Write(GpioAssignment.Backwards, PinValue.High);
                break;
            case Direction.Stop:
            default:
                _controller.Write(GpioAssignment.Backwards, PinValue.Low);
                _controller.Write(GpioAssignment.Forward, PinValue.Low);
                break;
        }
    }

    public void Dispose()
    {
        Stop();
        _controller?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}