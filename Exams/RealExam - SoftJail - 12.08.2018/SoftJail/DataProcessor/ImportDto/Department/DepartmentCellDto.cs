namespace SoftJail.DataProcessor.ImportDto.Department
{
    using System.ComponentModel.DataAnnotations;

    public class DepartmentCellDto
    {
        [Required]
        [Range(1, 1000)]
        public int CellNumber { get; set; }

        [Required]
        public bool HasWindow { get; set; }
    }
}