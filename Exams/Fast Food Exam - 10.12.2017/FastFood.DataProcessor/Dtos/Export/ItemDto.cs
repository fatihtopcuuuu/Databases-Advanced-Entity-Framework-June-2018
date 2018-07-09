namespace FastFood.DataProcessor.Dtos.Export
{
    using System.Xml.Serialization;

    [XmlType("MostPopularItem")]
    public class ItemDto
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("TotalMade")]
        public decimal TotalMade { get; set; }

        [XmlElement("TimesSold")]
        public int TimesSold { get; set; }
    }
}
