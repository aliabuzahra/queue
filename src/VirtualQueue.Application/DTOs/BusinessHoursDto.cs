namespace VirtualQueue.Application.DTOs;

public record BusinessHoursDto(
    string StartTime,
    string EndTime,
    List<int> WorkingDays, // 0=Sunday, 1=Monday, etc.
    string TimeZone
);
