using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using Mqtt;
using System.Reactive;
using MQTTnet.Client;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Exceptions;
using Avalonia.Threading;
using System.Collections.Generic;

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
            ZoneNumbers.Add(i);
        }
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
        if (index >= Jobs.Count)
        {
            Console.WriteLine($"Invalid index {index} for removing a job!  Not removing any jobs.");
            return; // @todo Throw an exception here instead, so that we can display an error message to the user.
        }
        var msg = new SprinklersCmdMsg();
        msg.Cmd = SprinklersCmdMsg.Command.DequeueJobByIndex;
        msg.Duration_s = index; ///< @todo Should we be borrowing this field? It shouldn't hurt anything, but it is lying...
        Console.WriteLine($"Removing Sprinkler Job #{index}");
        sprinklersCmdPub?.Publish(msg);
    }

    public void RequestSprinklerQueueStatus()
    {
        var msg = new SprinklersCmdMsg();
        msg.Cmd = SprinklersCmdMsg.Command.RequestQueueStatus;
        Console.WriteLine("Requesting current status of the sprinkler job queue.");
        if (sprinklersCmdPub is null)
        {
            Console.WriteLine("sprinklersCmdPub is null!  Not publishing request for queue status.");
            return; // @todo Throw an exception here instead, so that we can display an error message to the user.
        }
        sprinklersCmdPub.Publish(msg);
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
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Unable to connect to mqttClient (Operation Canceled)");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Caught some other exception: {e.Message}");
        }

        sprinklersCmdPub = new(mqttClient);
        sprinklersStatusSub = new(mqttClient);

        sprinklersStatusSub.Subscribe(onSprinklersStatusMsg);

        RequestSprinklerQueueStatus();
    }

    private void onSprinklersStatusMsg(string payload)
    {
        var msg = new SprinklersStatusMsg(payload);
        // Jobs = new ObservableCollection<SprinklerJob>(msg.Jobs);
        Jobs.Clear();
        foreach (var job in msg.Jobs)
        {
            Console.WriteLine($"Adding job: {job.ZoneNumber} for {job.Duration_s} seconds");
            Jobs.Add(job);
        }
        // try
        // {
        //     Dispatcher.UIThread.Post(() => SetJobs(msg.Jobs));
        //     // _ = Dispatcher.UIThread.InvokeAsync(GetText);
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine($"Caught exception while trying to update Jobs collection: {e.Message}");
        // }
    }

    public void SetJobs(List<SprinklerJob> updatedJobs)
    {
        Jobs.Clear();
        Jobs = new ObservableCollection<SprinklerJob>(updatedJobs);
    }


    private bool isMqttInitialized = false;
    private IMqttClient mqttClient;
    public Publisher<SprinklersCmdMsg>? sprinklersCmdPub;
    private Subscriber<SprinklersStatusMsg>? sprinklersStatusSub;

    public ObservableCollection<int> ZoneNumbers { get; set; } = [];
    public ObservableCollection<SprinklerJob> Jobs {get; set;} = [];
}
