namespace PetClinic.DataProcessor
{
    using AutoMapper;
    using Data;
    using Dtos.Import;
    using Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string FailureMessage = "Error: Invalid data.";
        private const string SuccessMessage = "Record {0} successfully imported.";

        public static string ImportAnimalAids(PetClinicContext context, string jsonString)
        {
            var animalAids = JsonConvert.DeserializeObject<AnimalAid[]>(jsonString);

            var validEntries = new List<AnimalAid>();

            var sb = new StringBuilder();

            foreach (var aa in animalAids)
            {
                var isValid = IsValid(aa);

                var alreadyExists = validEntries.Any(a => a.Name == aa.Name);

                if (!isValid || alreadyExists)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                validEntries.Add(aa);
                sb.AppendLine(string.Format(SuccessMessage, aa.Name));
            }

            context.AnimalAids.AddRange(validEntries);
            context.SaveChanges();

            var result = sb.ToString().TrimEnd();

            return result;
        }

        public static string ImportAnimals(PetClinicContext context, string jsonString)
        {
            var animals = JsonConvert.DeserializeObject<AnimalDto[]>(jsonString);

            var sb = new StringBuilder();
            var validEntries = new List<Animal>();

            foreach (var dto in animals)
            {
                var animal = Mapper.Map<Animal>(dto);

                var animalIsValid = IsValid(animal);
                var passportIsValid = IsValid(animal.Passport);

                var alreadyExists = validEntries.Any(a => a.Passport.SerialNumber == animal.Passport.SerialNumber);

                if (!animalIsValid || !passportIsValid || alreadyExists)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                validEntries.Add(animal);
                sb.AppendLine(string.Format(SuccessMessage, $"{animal.Name} Passport №: {animal.Passport.SerialNumber}"));
            }

            context.Animals.AddRange(validEntries);
            context.SaveChanges();

            var result = sb.ToString().TrimEnd();
            return result;
        }

        public static string ImportVets(PetClinicContext context, string xmlString)
        {
            var xDoc = XDocument.Parse(xmlString);
            var elements = xDoc.Root.Elements();

            var sb = new StringBuilder();
            var validEntries = new List<Vet>();

            foreach (var el in elements)
            {
                var name = el.Element("Name")?.Value;
                var profession = el.Element("Profession")?.Value;
                var ageString = el.Element("Age")?.Value;
                var phoneNumber = el.Element("PhoneNumber")?.Value;
                
                var age = 0;

                if (ageString != null)
                {
                    age = int.Parse(ageString);
                }

                var vet = new Vet
                {
                    Name = name,
                    Profession = profession,
                    Age = age,
                    PhoneNumber = phoneNumber
                };

                var isValid = IsValid(vet);
                var phoneNumberExists = validEntries.Any(v => v.PhoneNumber == vet.PhoneNumber);

                if (!isValid || phoneNumberExists)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                validEntries.Add(vet);
                sb.AppendLine(string.Format(SuccessMessage, vet.Name));
            }

            context.Vets.AddRange(validEntries);
            context.SaveChanges();

            var result = sb.ToString().TrimEnd();
            return result;
        }

        public static string ImportProcedures(PetClinicContext context, string xmlString)
        {
            var xDoc = XDocument.Parse(xmlString);
            var elements = xDoc.Root.Elements();

            var sb = new StringBuilder();
            var validEntries = new List<Procedure>();

            foreach (var el in elements)
            {
                var vetName = el.Element("Vet")?.Value;
                var passportId = el.Element("Animal")?.Value;
                var dateTimeString = el.Element("DateTime")?.Value;

                var vetId = context.Vets.SingleOrDefault(v => v.Name == vetName)?.Id;
                var passportExists = context.Passports.Any(p => p.SerialNumber == passportId);

                var dateIsValid = DateTime
                    .TryParseExact(dateTimeString, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);

                var animalAidElements = el.Element("AnimalAids")?.Elements();

                if (vetId == null || !passportExists || animalAidElements == null || !dateIsValid)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var animalAidIds = new List<int>();
                var allAidsExist = true;

                foreach (var aid in animalAidElements)
                {
                    var aidName = aid.Element("Name")?.Value;

                    var aidId = context.AnimalAids.SingleOrDefault(a => a.Name == aidName)?.Id;

                    if (aidId == null || animalAidIds.Any(id => id == aidId))
                    {
                        allAidsExist = false;
                        break;
                    }

                    animalAidIds.Add(aidId.Value);
                }

                if (!allAidsExist)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                var procedure = new Procedure
                {
                    VetId = vetId.Value,
                    AnimalId = context.Animals.Single(a => a.PassportSerialNumber == passportId).Id,
                    DateTime = dateTime
                };

                foreach (var id in animalAidIds)
                {
                    var mapping = new ProcedureAnimalAid
                    {
                        Procedure = procedure,
                        AnimalAidId = id
                    };

                    procedure.ProcedureAnimalAids.Add(mapping);
                }

                var isValid = IsValid(procedure);

                if (!isValid)
                {
                    sb.AppendLine(FailureMessage);
                    continue;
                }

                validEntries.Add(procedure);
                sb.AppendLine("Record successfully imported.");
            }

            context.Procedures.AddRange(validEntries);
            context.SaveChanges();

            var result = sb.ToString().TrimEnd();
            return result;
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