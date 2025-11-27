using System;
using System.Collections.Generic;

namespace DuLich_Tour.Models
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalTours { get; set; }
        public int TotalCustomers { get; set; }
        public List<DatTour> RecentBookings { get; set; }
        public List<decimal> MonthlyRevenue { get; set; }
        public List<string> RevenueLabels { get; set; }
    }
}
