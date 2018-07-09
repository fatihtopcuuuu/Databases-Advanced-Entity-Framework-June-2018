namespace Instagraph.DataProcessor.Dtos.Import
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    [XmlType("comment")]
    public class CommentDto
    {
        [XmlElement("content")]
        public string Content { get; set; }
        
        [XmlElement("user")]
        public string User { get; set; }
        
        [Required]
        [XmlElement("post")]
        public CommentPostDto PostId { get; set; }
    }
}