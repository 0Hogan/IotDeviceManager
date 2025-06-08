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
    }    

    public void OnPauseQueueClicked(object sender, RoutedEventArgs args)
    {
        if (this.DataContext is not null)
        {
            ((MainViewModel)this.DataContext).PauseSprinklerQueueExecution();
        }
    }

    public void OnResumeQueueClicked(object sender, RoutedEventArgs args)
    {
        if (this.DataContext is not null)
        {
            ((MainViewModel)this.DataContext).ResumeSprinklerQueueExecution();
        }
    }

    public void OnStopQueueClicked(object sender, RoutedEventArgs args)
    {
        if (this.DataContext is not null)
        {
            ((MainViewModel)this.DataContext).StopSprinklerQueueExecution();
        }
    }

    public void OnRemoveJobClicked(object sender, RoutedEventArgs args)
    {
        if (this.DataContext is not null)
        {
            if (CurrentJobs.SelectedItem is null)
            {
                Console.WriteLine("No job selected to remove!");
                return; // @todo Display an error message to the user.
            }
            else
            {
                UInt16 jobIndex = Convert.ToUInt16(CurrentJobs.SelectedIndex);
                Console.WriteLine($"Removing job at index {jobIndex}.");
                ((MainViewModel)this.DataContext).RemoveSprinklerJobByIndex(jobIndex);
            }
        }
    }

    public void OnRefreshJobsClicked(object sender, RoutedEventArgs args)
    {
        if (this.DataContext is not null)
        {
            ((MainViewModel)this.DataContext).RequestSprinklerQueueStatus();
        }
    }
}