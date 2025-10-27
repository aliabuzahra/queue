namespace VirtualQueue.Application.DTOs;

public record QueueScheduleDto(
    BusinessHoursDto? BusinessHours,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsRecurring,
    List<DateTime>? SpecificDates
);
