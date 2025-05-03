using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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


        public string? ImagePath { get; set; }

        [NotMapped]
        [ValidateNever] // ekstra koruma
        public IFormFile? ImageFile { get; set; }

        public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        [ValidateNever]
        public User Owner { get; set; }


        [ValidateNever]
        public ICollection<Reservation> Reservations { get; set; }

        [ValidateNever]
        public ICollection<HouseAvailability> Availabilities { get; set; } = new List<HouseAvailability>();

    }
}
