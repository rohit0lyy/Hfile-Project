using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HFile.Models
{
    public class MedicalFileUploadModel
    {
        [Key] // Primary Key
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select file type.")]
        [RegularExpression("Lab Report|Prescription|X-Ray|Blood Report|MRI Scan|CT Scan",
            ErrorMessage = "Invalid File Type selected.")]
        public string? FileType { get; set; }

        [Required(ErrorMessage = "Please enter file name.")]
        [StringLength(100, ErrorMessage = "File name cannot exceed 100 characters.")]
        public string? FileName { get; set; }

        [NotMapped] // EF Core ko ignore karne ke liye
        [Required(ErrorMessage = "Please choose a file to upload.")]
        [DataType(DataType.Upload)]
        public IFormFile FileUpload { get; set; }

        [StringLength(250)]
        public string? FilePath { get; set; } // DB me file ka path store
    }
}
