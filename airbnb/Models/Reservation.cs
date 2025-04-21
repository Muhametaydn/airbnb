using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace airbnb.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        // Foreign Key → House
        [Required]
        public int HouseId { get; set; }

        [ForeignKey("HouseId")]
        public House House { get; set; }

        // Foreign Key → Tenant (User)
        [Required]
        public int TenantId { get; set; }

        [ForeignKey("TenantId")]
        public User Tenant { get; set; }

        // Dates
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Optional navigation properties
        public Payment? Payment { get; set; }
        public Review? Review { get; set; }
    }
}
