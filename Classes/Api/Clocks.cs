using System.Xaml;

namespace sambar;

public partial class Api
{
    public delegate void ClockTickedEventHandler(Time time);
    public event ClockTickedEventHandler CLOCK_TICKED = (time) => { };
    private void ClockInit()
    {
        Task.Run(async () =>
        {
            while(true)
            {
                await Task.Delay(60000);
            }
        });
    }

    // Api Endpoint
    // Set polling frequence / event firing rate
}

public class Time
{
    public int seconds;
    public int minutes;
    public int hours;
    public int day;
    public int month;
    public int year;
}

