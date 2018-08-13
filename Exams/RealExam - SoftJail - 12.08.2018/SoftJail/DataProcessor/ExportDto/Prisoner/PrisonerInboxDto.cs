namespace SoftJail.DataProcessor.ExportDto.Prisoner
{
    using System.Xml.Serialization;

    [XmlType("Prisoner")]
    public class PrisonerInboxDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string IncarcerationDate { get; set; }

        public EncryptedMessageDto[] EncryptedMessages { get; set; }
    }
}
