using AutoMapper;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Entities;

namespace VirtualQueue.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Tenant, TenantDto>();
        CreateMap<Queue, QueueDto>();
        CreateMap<UserSession, UserSessionDto>();
    }
}
