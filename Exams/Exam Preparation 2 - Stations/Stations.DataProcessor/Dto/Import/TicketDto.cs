namespace Stations.DataProcessor.Dto.Import
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    [XmlType("Ticket")]
    public class TicketDto
    {
        [Required]
        [XmlAttribute("price")]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public string Price { get; set; }

        [Required]
        [XmlAttribute("seat")]
        [RegularExpression("^([A-Za-z]{2})([0-9]{1,6})$")]
        public string Seat { get; set; }

        [Required]
        public TicketTripDto Trip { get; set; }

        public TicketCardDto Card { get; set; }
    }
}