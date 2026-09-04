namespace QueueLess.Application.DTOs.F_Services;

public record UpdateServiceRequest(
    string Name,
    string? Description,
    int EstimatedDurationMinutes
);