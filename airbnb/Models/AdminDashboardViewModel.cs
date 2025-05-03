public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalHouses { get; set; }
    public int TotalReservations { get; set; }

    public decimal TotalPayments { get; set; }         // isPaid = true
    public decimal CancelledPayments { get; set; }     // isPaid = false
    public decimal Profit { get; set; }                // Kar payı (örneğin %10)



}
