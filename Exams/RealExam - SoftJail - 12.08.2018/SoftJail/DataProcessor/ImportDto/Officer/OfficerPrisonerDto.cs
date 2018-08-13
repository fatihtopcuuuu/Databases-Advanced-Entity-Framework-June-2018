namespace SoftJail.DataProcessor.ImportDto.Officer
{
    using System.Xml.Serialization;

    [XmlType("Prisoner")]
    public class OfficerPrisonerDto
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}