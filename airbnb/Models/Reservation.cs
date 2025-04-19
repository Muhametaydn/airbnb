using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace airbnb.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        public int HouseId { get; set; }

        [ForeignKey("HouseId")]
        public House House { get; set; }

        public int TenantId { get; set; }

        [ForeignKey("TenantId")]
        public User Tenant { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Payment Payment { get; set; }
        public Review Review { get; set; }
    }
}
