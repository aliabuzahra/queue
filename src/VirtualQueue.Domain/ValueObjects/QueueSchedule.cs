namespace VirtualQueue.Domain.ValueObjects;

public class QueueSchedule
{
    public BusinessHours? BusinessHours { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsRecurring { get; private set; }
    public List<DateTime> SpecificDates { get; private set; }

    private QueueSchedule() { } // EF Core constructor

    public QueueSchedule(
        BusinessHours? businessHours = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool isRecurring = false,
        List<DateTime>? specificDates = null)
    {
        BusinessHours = businessHours;
        StartDate = startDate;
        EndDate = endDate;
        IsRecurring = isRecurring;
        SpecificDates = specificDates ?? new List<DateTime>();
    }

    public bool IsQueueActive(DateTime dateTime)
    {
        // Check if within date range
        if (StartDate.HasValue && dateTime < StartDate.Value)
            return false;
            
        if (EndDate.HasValue && dateTime > EndDate.Value)
            return false;

        // Check specific dates
        if (SpecificDates.Any())
        {
            return SpecificDates.Any(d => d.Date == dateTime.Date);
        }

        // Check business hours
        if (BusinessHours != null)
        {
            return BusinessHours.IsWithinBusinessHours(dateTime);
        }

        return true; // No restrictions
    }

    public DateTime? GetNextActivationTime(DateTime fromDateTime)
    {
        if (SpecificDates.Any())
        {
            var nextDate = SpecificDates
                .Where(d => d > fromDateTime)
                .OrderBy(d => d)
                .FirstOrDefault();
            return nextDate;
        }

        if (BusinessHours != null)
        {
            return BusinessHours.GetNextBusinessDay(fromDateTime);
        }

        return null;
    }

    public DateTime? GetPreviousActivationTime(DateTime fromDateTime)
    {
        if (SpecificDates.Any())
        {
            var previousDate = SpecificDates
                .Where(d => d < fromDateTime)
                .OrderByDescending(d => d)
                .FirstOrDefault();
            return previousDate;
        }

        if (BusinessHours != null)
        {
            return BusinessHours.GetPreviousBusinessDay(fromDateTime);
        }

        return null;
    }
}
