using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace airbnb.Models
{
    public class HouseAvailability
    {
        [Key]
        public int AvailabilityId { get; set; }

        public int HouseId { get; set; }

        [ForeignKey("HouseId")]
        public House House { get; set; }

        public DateTime AvailableFrom { get; set; }
        public DateTime AvailableTo { get; set; }
    }
}
