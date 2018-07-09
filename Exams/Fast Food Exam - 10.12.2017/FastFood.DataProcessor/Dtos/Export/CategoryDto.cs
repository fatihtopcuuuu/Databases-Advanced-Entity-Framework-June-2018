namespace FastFood.DataProcessor.Dtos.Export
{
    using Import;
    using System.Xml.Serialization;

    [XmlType("Category")]
    public class CategoryDto
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("MostPopularItem")]
        public ItemDto MostPopularItem { get; set; }
    }
}
