﻿namespace Instagraph.DataProcessor.Dtos.Import
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    [XmlType("post")]
    public class PostDto
    {
        [Required]
        [XmlElement("caption")]
        public string Caption { get; set; }

        [XmlElement("user")]
        public string User { get; set; }

        [XmlElement("picture")]
        public string Picture { get; set; }
    }
}