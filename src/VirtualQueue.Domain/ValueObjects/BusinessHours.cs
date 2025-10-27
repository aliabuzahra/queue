namespace VirtualQueue.Domain.ValueObjects;

public class BusinessHours
{
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public List<DayOfWeek> WorkingDays { get; private set; }
    public string TimeZone { get; private set; }

    private BusinessHours() { } // EF Core constructor

    public BusinessHours(TimeSpan startTime, TimeSpan endTime, List<DayOfWeek> workingDays, string timeZone = "UTC")
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        if (workingDays == null || !workingDays.Any())
            throw new ArgumentException("At least one working day must be specified");

        StartTime = startTime;
        EndTime = endTime;
        WorkingDays = workingDays;
        TimeZone = timeZone;
    }

    public bool IsWithinBusinessHours(DateTime dateTime)
    {
        var timeInZone = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, TimeZone);
        var dayOfWeek = timeInZone.DayOfWeek;
        var timeOfDay = timeInZone.TimeOfDay;

        return WorkingDays.Contains(dayOfWeek) && 
               timeOfDay >= StartTime && 
               timeOfDay <= EndTime;
    }

    public DateTime GetNextBusinessDay(DateTime fromDate)
    {
        var current = fromDate.Date.AddDays(1);
        
        while (!WorkingDays.Contains(current.DayOfWeek))
        {
            current = current.AddDays(1);
        }
        
        return current.Date.Add(StartTime);
    }

    public DateTime GetPreviousBusinessDay(DateTime fromDate)
    {
        var current = fromDate.Date.AddDays(-1);
        
        while (!WorkingDays.Contains(current.DayOfWeek))
        {
            current = current.AddDays(-1);
        }
        
        return current.Date.Add(StartTime);
    }
}
