using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace airbnb.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}
