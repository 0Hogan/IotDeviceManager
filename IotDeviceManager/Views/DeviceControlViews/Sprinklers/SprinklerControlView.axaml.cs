using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IotDeviceManager.ViewModels;
using Mqtt;

namespace IotDeviceManager.Views;

public partial class SprinklerControlView : UserControl
{
    public SprinklerControlView()
    {
        InitializeComponent();
    }

    public void OnSubmitJobClicked(object sender, RoutedEventArgs args)
    {
        UInt16 zoneNumber = Convert.ToUInt16(ZoneNumberComboBox.SelectedItem ?? 0xff);
        UInt64 duration_s = Convert.ToUInt64((DurationNumericUpDown.Value ?? 0) * 60);
        
        if (this.DataContext is not null)
        {
            ((MainViewModel)this.DataContext).SubmitSprinklerJob(zoneNumber, duration_s);
        }
        // MainViewModel vm = 
        // // No need to use the following if I can send the above into the ViewModel...
        // SprinklersCmdMsg msg = new();
        // msg.Cmd = SprinklersCmdMsg.Command.EnqueueJob;
        // msg.ZoneNumber = zoneNumber;
        // msg.Duration_s = duration_s;
        
        // Console.WriteLine(msg.GetPayload());
        // Console.WriteLine($"Submitting job to run zone #{zoneNumber} for {duration_s} minutes.");
    }    
}