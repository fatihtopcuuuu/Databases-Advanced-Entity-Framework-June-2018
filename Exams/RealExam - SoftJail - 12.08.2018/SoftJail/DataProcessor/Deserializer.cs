namespace SoftJail.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using AutoMapper;
    using Data;
    using Data.Models;
    using Data.Models.Enums;
    using ImportDto.Department;
    using ImportDto.Officer;
    using ImportDto.Prisoner;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string FailureMessage = "Invalid Data";
        private const string RootElementName = "Officers";
        private const string DateTimeFormat = "dd/MM/yyyy";

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedDepartments = JsonConvert.DeserializeObject<DepartmentDto[]>(jsonString);

            var validDepartments = new List<Department>();
            foreach (var departmentDto in deserializedDepartments)
            {
                if (!IsValid(departmentDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                bool hasInvalidCells = !departmentDto.Cells.All(IsValid);

                if (hasInvalidCells)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var department = Mapper.Map<Department>(departmentDto);

                validDepartments.Add(department);
                sb.AppendLine($"Imported {departmentDto.Name} with {departmentDto.Cells.Length} cells");
            }

            context.Departments.AddRange(validDepartments);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var deserializedPrisoners = JsonConvert.DeserializeObject<PrisonerDto[]>(jsonString);

            var validPrisoners = new List<Prisoner>();
            foreach (var prisonerDto in deserializedPrisoners)
            {
                if (!IsValid(prisonerDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var releaseDate = prisonerDto.ReleaseDate != null
                    ? new DateTime?(DateTime.ParseExact(prisonerDto.ReleaseDate, DateTimeFormat,
                        CultureInfo.InvariantCulture))
                    : null;

                bool hasInvalidMails = !prisonerDto.Mails.All(IsValid);

                if (hasInvalidMails)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var prisoner = Mapper.Map<Prisoner>(prisonerDto);
                prisoner.ReleaseDate = releaseDate;


                validPrisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisonerDto.FullName} {prisonerDto.Age} years old");
            }

            context.Prisoners.AddRange(validPrisoners);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(OfficerDto[]), new XmlRootAttribute(RootElementName));
            var deserializeOfficerPrisoners =
                (OfficerDto[])serializer.Deserialize(new StringReader(xmlString));

            var validOfficers = new List<Officer>();
            foreach (var officerPrisonerDto in deserializeOfficerPrisoners)
            {
                if (!IsValid(officerPrisonerDto))
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                bool hasInvalidPrisoners = false;
                var validPrisoners = new List<OfficerPrisoner>();
                foreach (var prisonerDto in officerPrisonerDto.Prisoners)
                {
                    if (!IsValid(prisonerDto))
                    {
                        hasInvalidPrisoners = true;
                        break;
                    }

                    var officerPrisoner = new OfficerPrisoner
                    {
                        PrisonerId = prisonerDto.Id,
                    };

                    validPrisoners.Add(officerPrisoner);
                }

                if (hasInvalidPrisoners)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var isPositionValid = Enum.TryParse(officerPrisonerDto.Position, out Position pos);
                var isWeaponValid = Enum.TryParse(officerPrisonerDto.Weapon, out Weapon weap);

                if (!isPositionValid || !isWeaponValid)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var officer = new Officer
                {
                    DepartmentId = officerPrisonerDto.DepartmentId,
                    FullName = officerPrisonerDto.Name,
                    Position = pos,
                    Salary = officerPrisonerDto.Money,
                    Weapon = weap,
                    OfficerPrisoners = validPrisoners,
                };

                validOfficers.Add(officer);
                sb.AppendLine($"Imported {officerPrisonerDto.Name} ({officerPrisonerDto.Prisoners.Length} prisoners)");
            }

            context.Officers.AddRange(validOfficers);
            context.SaveChanges();

            return sb.ToString();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new ValidationContext(obj);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);

            return isValid;
        }
    }
}