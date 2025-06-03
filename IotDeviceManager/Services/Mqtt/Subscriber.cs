using System;
using System.Text;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;

namespace Mqtt;

public class Subscriber<msgType> where msgType : Message, new()
{

    public Subscriber(IMqttClient client)
    {
        mqttClient = client;
        Topic = new msgType().GetTopic();
    }

    public async void Subscribe(Action<string> msgCallback)
    {
        try
        {
            await mqttClient.SubscribeAsync(Topic);

            /// @note This lambda must be asnychronous to get added to the list of callbacks for the
            ///       MQTT client, but there doesn't need to be a return type for an MQTT callback.
            ///       Thus, we have the warning that the following async method will run synchronously.
            /// @todo Figure out how to fix this in a proper way rather than just ignoring it.
            #pragma warning disable CS1998
            mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    var msgPayload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                    msgCallback(msgPayload);
                };
            #pragma warning restore CS1998
        }
        catch (MqttClientNotConnectedException)
        {
            Console.WriteLine("The MQTT Client wasn't connected!");
        }
    }

    public string Topic { get; }
    private IMqttClient mqttClient;
}