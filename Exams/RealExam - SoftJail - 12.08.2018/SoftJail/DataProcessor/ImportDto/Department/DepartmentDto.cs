namespace SoftJail.DataProcessor.ImportDto.Department
{
    using System.ComponentModel.DataAnnotations;

    public class DepartmentDto
    {
        [Required]
        [StringLength(25, MinimumLength = 3)]
        public string Name { get; set; }

        public DepartmentCellDto[] Cells { get; set; }
    }
}
