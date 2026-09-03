using QueueLess.Domain.Enums;

namespace QueueLess.Application.DTOs.Facility;
public record UpdateFacilityRequest(
    string Name,
    string? Description,
    string Location,
    string OperatingHours
);