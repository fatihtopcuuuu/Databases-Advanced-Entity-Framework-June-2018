namespace SoftJail.DataProcessor
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using ExportDto;
    using ExportDto.Prisoner;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        private const string DateTimeFormat = "yyyy-MM-dd";
        private const string RootElementName = "Prisoners";

        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context
                .Prisoners
                .Where(p => ids.Any(i => p.Id == i))
                .Select(p => new PrisonerDto
                {
                    Id = p.Id,
                    Name = p.FullName,
                    CellNumber = p.Cell.CellNumber,
                    Officers = p.PrisonerOfficers.Select(o => new PrisonerOfficerDto
                    {
                        OfficerName = o.Officer.FullName,
                        Department = o.Officer.Department.Name,
                    })
                        .OrderBy(o => o.OfficerName)
                        .ToArray(),
                    TotalOfficerSalary = p.PrisonerOfficers.Sum(o => o.Officer.Salary),
                })
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToList();

            var json = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return json;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var sb = new StringBuilder();

            var names = prisonersNames.Split(',');

            var prisoners = context
                .Prisoners
                .Where(p => names.Contains(p.FullName))
                .Select(p => new PrisonerInboxDto
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                    EncryptedMessages = p.Mails.Select(m => new EncryptedMessageDto
                    {
                        Description = m.Description.ReverseString(),
                    })
                        .ToArray()
                })
                .OrderBy(p => p.Name)
                .ThenBy(p => p.Id)
                .ToArray();

            var serializer = new XmlSerializer(typeof(PrisonerInboxDto[]), new XmlRootAttribute(RootElementName));
            serializer.Serialize(new StringWriter(sb),
                prisoners,
                new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty, }));

            return sb.ToString();
        }

    }
}