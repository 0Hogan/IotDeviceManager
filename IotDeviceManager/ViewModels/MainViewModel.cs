using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using Mqtt;
using System.Reactive;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Exceptions;

namespace IotDeviceManager.ViewModels;

public class MainViewModel : ViewModelBase
{
    // public ReactiveCommand<Object, Unit> SubmitNewSprinklerJob { get; }
    public MainViewModel()
    {
        /// I'm not convinced these collections shouldn't be in the Model folder instead of the
        ///    ViewModel folder... Ah well. Good enough for now, but look more into the MVVM arch.
        /// 1. Get available zones and populate ZoneNumbers. For initial use, we can probably just
        ///    grab this from the MCU or hardcode it. Long-term, we probably want to do a fetch
        ///    from the MCU and store some sort of mapping that allows us to give labels or
        ///    descriptions to each zone.
        /// 2. Get any jobs currently in progress and populate Jobs.
        ///    @note We might want some way differentiate between jobs that are confirmed to be
        ///          queued up on the MCU and jobs that are still being submitted to the queue.
        /// 3. Consider how things should be handled if there are multiple sprinkler control
        ///    devices...

        var mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateMqttClient();
        _ = initMqtt();


        for (UInt16 i = 1; i < 10; i++)
        {
            Jobs.Add(new SprinklerJob(i, 5*60));
            ZoneNumbers.Add(i);
        }

        // SubmitNewSprinklerJob = ReactiveCommand.Create<Object>(SubmitSprinklerJob);
    }

    public void SubmitSprinklerJob(UInt16 zoneNumber, UInt64 duration_s)
    {
        if (zoneNumber == 0xff)
        {
            Console.WriteLine($"zoneNumber wasn't specified!  Not enqueing this job (zoneNumber={zoneNumber}; duration_s={duration_s})!");
            return; /// @todo Throw an exception here instead, so that we can display an error message to the user.
        }
        if (duration_s == 0)
        {
            Console.WriteLine($"duration_s was 0! Not enqueing this job (zoneNumber={zoneNumber}; duration_s={duration_s})!");
            return; /// @todo Throw an exception here instead, so that we can display an error message to the user.
        }

        var msg = new SprinklersCmdMsg();
        msg.Cmd = SprinklersCmdMsg.Command.EnqueueJob;
        msg.ZoneNumber = zoneNumber;
        msg.Duration_s = duration_s;
        Console.WriteLine("SprinklersCmdMsg:");
        Console.WriteLine(msg.GetPayload());
        sprinklersCmdPub?.Publish(msg);
    }

    public void PauseSprinklerQueueExecution()
    {
        var msg = new SprinklersCmdMsg();
        msg.Cmd = SprinklersCmdMsg.Command.PauseQueueExecution;
        Console.WriteLine("Pausing execution of queued sprinkler jobs.");
        sprinklersCmdPub?.Publish(msg);
    }

    public void ResumeSprinklerQueueExecution()
    {
        var msg = new SprinklersCmdMsg();
        msg.Cmd = SprinklersCmdMsg.Command.ResumeQueueExecution;
        Console.WriteLine("Resuming execution of queued sprinkler jobs.");
        sprinklersCmdPub?.Publish(msg);
    }

    public void StopSprinklerQueueExecution()
    {
        var msg = new SprinklersCmdMsg();
        msg.Cmd = SprinklersCmdMsg.Command.StopQueueExecution;
        Console.WriteLine("Stopping all queued sprinkler jobs.");
        sprinklersCmdPub?.Publish(msg);
    }

    public void RemoveSprinklerJobByIndex(UInt16 index)
    {
        /// @todo Ensure there are at least index + 1 jobs.
        var msg = new SprinklersCmdMsg();
        msg.Cmd = SprinklersCmdMsg.Command.DequeueJobByIndex; ///< @todo Add DequeueJobByIndex Command enum.
        msg.Duration_s = index; ///< @todo Should we be borrowing this field? It shouldn't hurt anything, but it is lying...
        Console.WriteLine($"Removing Sprinkler Job #{index}");
        sprinklersCmdPub?.Publish(msg);
    }

    private async Task initMqtt()
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
                                    .WithTcpServer(MqttServerInfo.Address, MqttServerInfo.Port)
                                    .WithCredentials(MqttServerInfo.Username, MqttServerInfo.Password)
                                    .Build();

        try
        {
            if (mqttClient is null)
            {
                return;
            }
            var connectResult = await mqttClient.ConnectAsync(mqttClientOptions);
            isMqttInitialized = connectResult.ResultCode == MqttClientConnectResultCode.Success;
        }
        catch(OperationCanceledException)
        {
            Console.WriteLine($"Unable to connect to mqttClient (Operation Canceled)");
        }
        catch(Exception e)
        {
            Console.WriteLine($"Caught some other exception: {e.Message}");
        }

        sprinklersCmdPub = new(mqttClient);
        sprinklersStatusSub = new(mqttClient);

        sprinklersStatusSub.Subscribe(onSprinklersStatusMsg);
    }

    private void onSprinklersStatusMsg(string payload)
    {
        Console.WriteLine($"I heard: \"{payload}\"");
    }

    private bool testMode = true;

    private bool isMqttInitialized = false;
    private IMqttClient mqttClient;
    public Publisher<SprinklersCmdMsg>? sprinklersCmdPub;
    private Subscriber<SprinklersStatusMsg>? sprinklersStatusSub;

    public ObservableCollection<int> ZoneNumbers { get; set; } = [];
    public ObservableCollection<SprinklerJob> Jobs {get; set;} = [];
}
