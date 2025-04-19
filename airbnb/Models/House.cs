using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace airbnb.Models
{
    public class House
    {
        [Key]
        public int HouseId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        [Required]
        public string Location { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public ICollection<Reservation> Reservations { get; set; }
        public ICollection<HouseAvailability> Availabilities { get; set; }
    }
}
