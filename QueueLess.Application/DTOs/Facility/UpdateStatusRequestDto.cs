using QueueLess.Domain.Enums;
namespace QueueLess.Application.DTOs.Facility;

public record UpdateFacilityStatusRequest(QueueStatus Status);