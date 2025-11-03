using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HFile.Models
{
    public class UploadFileModels
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string? FileName { get; set; }
        public string? File_extension { get; set; }

        public int Uploaded_by { get; set; }
        public DateTime Uploaded_Date { get; set; }
        public Boolean IsDeleted { get; set; }
    }
}
