using AutoMapper;
using FamilyVault.Application.DTOs.Audit;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Mappings;

/// <summary>
/// Represents AuditMapping.
/// </summary>
public class AuditMapping : Profile
{
    /// <summary>
    /// Initializes a new instance of AuditMapping.
    /// </summary>
    public AuditMapping()
    {
        CreateMap<AuditEvent, AuditEventDto>().ReverseMap();
    }
}
