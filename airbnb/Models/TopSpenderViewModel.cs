namespace airbnb.Models
{
    public class TopSpenderViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
