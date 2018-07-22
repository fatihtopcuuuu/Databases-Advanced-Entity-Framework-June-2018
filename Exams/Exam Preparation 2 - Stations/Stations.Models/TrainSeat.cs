namespace Stations.Models
{
    using System.ComponentModel.DataAnnotations;

    public class TrainSeat
    {
        public int Id { get; set; }

        [Required]
        public int TrainId { get; set; }

        public Train Train { get; set; }

        [Required]
        public int SeatingClassId { get; set; }

        public SeatingClass SeatingClass { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}