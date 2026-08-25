namespace QueueLess.Application.DTOs.Admin;

public class AdminDashboardStatsDto
{
    public int ActiveFacilities { get; set; }
    public int ActiveServices { get; set; }
    public int ActiveStaff { get; set; }
    public int CustomersWaiting { get; set; }
    public int CustomersServedToday { get; set; }
}