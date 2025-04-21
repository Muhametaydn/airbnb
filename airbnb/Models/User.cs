using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace airbnb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        [ValidateNever]
        public Role Role { get; set; }


        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public ICollection<Reservation> ReservationsAsTenant { get; set; }
    }
}
