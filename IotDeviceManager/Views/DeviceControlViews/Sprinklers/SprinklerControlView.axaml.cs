using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using IotDeviceManager.ViewModels;

namespace IotDeviceManager.Views;

public partial class SprinklerControlView : UserControl
{
    public SprinklerControlView()
    {
        InitializeComponent();
    }

    public void OnSubmitJobClicked(object sender, RoutedEventArgs args)
    {
        // UInt16 zoneNumber = ZoneNumberComboBox.SelectedItem;
        Console.WriteLine($"Submitting job to run zone #{ZoneNumberComboBox.SelectedItem} for {DurationNumericUpDown.Value} minutes.");
    }
}