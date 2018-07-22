namespace Stations.DataProcessor.Dto.Import
{
    using System.Xml.Serialization;

    [XmlType("Card")]
    public class TicketCardDto
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }
}