using QueueLess.Domain.Entities;
using System.Collections.Generic;

namespace QueueLess.Application.DTOs.Staff;

public class StaffDashboardDto
{
    public Ticket? CurrentlyServing { get; set; }
    public IEnumerable<Ticket> WaitingList { get; set; } = new List<Ticket>();
    public IEnumerable<Ticket> RecentActivity { get; set; } = new List<Ticket>();
}