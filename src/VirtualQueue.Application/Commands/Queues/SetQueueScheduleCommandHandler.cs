using AutoMapper;
using MediatR;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.ValueObjects;

namespace VirtualQueue.Application.Commands.Queues;

public class SetQueueScheduleCommandHandler : IRequestHandler<SetQueueScheduleCommand, QueueDto>
{
    private readonly IQueueRepository _queueRepository;
    private readonly IMapper _mapper;

    public SetQueueScheduleCommandHandler(IQueueRepository queueRepository, IMapper mapper)
    {
        _queueRepository = queueRepository;
        _mapper = mapper;
    }

    public async Task<QueueDto> Handle(SetQueueScheduleCommand request, CancellationToken cancellationToken)
    {
        var queue = await _queueRepository.GetByTenantIdAndIdAsync(request.TenantId, request.QueueId, cancellationToken);
        if (queue == null)
        {
            throw new InvalidOperationException($"Queue with ID '{request.QueueId}' not found for tenant");
        }

        var schedule = MapToQueueSchedule(request.Schedule);
        queue.SetSchedule(schedule);

        await _queueRepository.UpdateAsync(queue, cancellationToken);
        await _queueRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<QueueDto>(queue);
    }

    private static QueueSchedule MapToQueueSchedule(QueueScheduleDto dto)
    {
        BusinessHours? businessHours = null;
        if (dto.BusinessHours != null)
        {
            var workingDays = dto.BusinessHours.WorkingDays.Select(d => (DayOfWeek)d).ToList();
            businessHours = new BusinessHours(
                TimeSpan.Parse(dto.BusinessHours.StartTime),
                TimeSpan.Parse(dto.BusinessHours.EndTime),
                workingDays,
                dto.BusinessHours.TimeZone);
        }

        return new QueueSchedule(
            businessHours,
            dto.StartDate,
            dto.EndDate,
            dto.IsRecurring,
            dto.SpecificDates);
    }
}
