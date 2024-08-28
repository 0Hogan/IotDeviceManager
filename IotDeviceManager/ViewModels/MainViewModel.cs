using System.Collections.ObjectModel;

namespace IotDeviceManager.ViewModels;

public class MainViewModel : ViewModelBase
{
// #pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
// #pragma warning restore CA1822 // Mark members as static

    public MainViewModel()
    {

        for (int i = 1; i < 10; i++)
        {
            Jobs.Add(new SprinklerJob(i, 5*60));
            ZoneNumbers.Add(i);
        }
    }

    public ObservableCollection<int> ZoneNumbers { get; set; } = [];
    public ObservableCollection<SprinklerJob> Jobs {get; set;} = [];
}
