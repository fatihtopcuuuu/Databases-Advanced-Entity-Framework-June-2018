namespace SoftJail.DataProcessor.ImportDto.Prisoner
{
    using System.ComponentModel.DataAnnotations;

    public class PrisonerMailDto
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public string Sender { get; set; }

        [Required]
        [RegularExpression(@"^([A-Za-z0-9\s]+) str.$")]
        public string Address { get; set; }
    }
}