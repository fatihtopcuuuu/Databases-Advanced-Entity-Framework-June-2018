namespace PetClinic.App
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using AutoMapper;
    using Data;
    using DataProcessor;
    using Microsoft.EntityFrameworkCore;

    public class StartUp
    {
        static void Main()
        {
            using (var context = new PetClinicContext())
            {
                Mapper.Initialize(config => config.AddProfile<PetClinicProfile>());

                ResetDatabase(context, true);
                Console.WriteLine("Database Reset");

                ImportEntities(context);

                ExportEntities(context);

                BonusTask(context);
            }
        }

        private static void ImportEntities(PetClinicContext context, string baseDir = @"..\../../../Datasets\")
        {
            const string exportDir = "./../../../App/Results/";

            string animalAids = Deserializer.ImportAnimalAids(context, File.ReadAllText(baseDir + "animalAids.json"));
            PrintAndExportEntityToFile(animalAids, exportDir + "AnimalAidsImport.txt");

            string animals = Deserializer.ImportAnimals(context, File.ReadAllText(baseDir + "animals.json"));
            PrintAndExportEntityToFile(animals, exportDir + "AnimalsImport.txt");

            string vets = Deserializer.ImportVets(context, File.ReadAllText(baseDir + "vets.xml"));
            PrintAndExportEntityToFile(vets, exportDir + "VetsImport.txt");

            var procedures = Deserializer.ImportProcedures(context, File.ReadAllText(baseDir + "procedures.xml"));
            PrintAndExportEntityToFile(procedures, exportDir + "ProceduresImport.txt");
        }

        private static void ExportEntities(PetClinicContext context)
        {
            const string exportDir = "./../../../App/Results/";

            string animalsExport = Serializer.ExportAnimalsByOwnerPhoneNumber(context, "0887446123");
            PrintAndExportEntityToFile(animalsExport, exportDir + "AnimalsExport.json");

            string proceduresExport = Serializer.ExportAllProcedures(context);
            PrintAndExportEntityToFile(proceduresExport, exportDir + "ProceduresExport.xml");
        }

        private static void BonusTask(PetClinicContext context)
        {
            var bonusOutput = Bonus.UpdateVetProfession(context, "+359284566778", "Primary Care");
            Console.WriteLine(bonusOutput);
        }

        private static void PrintAndExportEntityToFile(string entityOutput, string outputPath)
        {
            Console.WriteLine(entityOutput);
            File.WriteAllText(outputPath, entityOutput.TrimEnd());
        }

        private static void ResetDatabase(PetClinicContext context, bool shouldDeleteDatabase = false)
        {
            if (shouldDeleteDatabase)
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            context.Database.EnsureCreated();

            var disableIntegrityChecksQuery = "EXEC sp_MSforeachtable @command1='ALTER TABLE ? NOCHECK CONSTRAINT ALL'";
            context.Database.ExecuteSqlCommand(disableIntegrityChecksQuery);

            var deleteRowsQuery = "EXEC sp_MSforeachtable @command1='DELETE FROM ?'";
            context.Database.ExecuteSqlCommand(deleteRowsQuery);

            var enableIntegrityChecksQuery = "EXEC sp_MSforeachtable @command1='ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'";
            context.Database.ExecuteSqlCommand(enableIntegrityChecksQuery);

            var reseedQuery = "EXEC sp_MSforeachtable @command1='DBCC CHECKIDENT(''?'', RESEED, 0)'";
            try
            {
                context.Database.ExecuteSqlCommand(reseedQuery);
            }
            catch (SqlException)
            {
            }
        }
    }
}
