namespace Instagraph.DataProcessor.Dtos.Import
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    public class CommentPostDto
    {
        [Required]
        [XmlAttribute("id")]
        public string Id { get; set; }
    }
}