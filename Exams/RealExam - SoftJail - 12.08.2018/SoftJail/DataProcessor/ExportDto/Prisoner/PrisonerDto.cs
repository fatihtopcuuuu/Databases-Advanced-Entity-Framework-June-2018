namespace SoftJail.DataProcessor.ExportDto.Prisoner
{
    public class PrisonerDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CellNumber { get; set; }

        public PrisonerOfficerDto[] Officers { get; set; }

        public decimal TotalOfficerSalary { get; set; }
    }
}
