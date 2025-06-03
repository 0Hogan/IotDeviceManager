using System;
using MQTTnet.Client;
// using System.Timers;

namespace Mqtt;

/**
 * @brief 
 *
 * @todo Still need to figure out what Msg contents should be published...
 *
*/
public class PeriodicPublisher<msgType> : Publisher<msgType> where msgType : Message, new()
{
    public PeriodicPublisher(IMqttClient client, double timeBetweenMsgs, bool start = true) : base(client)
    {
        timer = new(timeBetweenMsgs);
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = true;
        timer.Enabled = start;
    }

    public void Start()
    {
        timer.Enabled = true;
    }

    public void Stop()
    {
        timer.Enabled = false;
    }

    private void OnTimedEvent(Object? src, System.Timers.ElapsedEventArgs e)
    {
        PrepMsgCallback?.Invoke();
        Publish(Msg);
    }

    private System.Timers.Timer timer;
    public Action? PrepMsgCallback;
    public msgType Msg { get; set; } = new msgType();
}