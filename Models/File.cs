using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqFiles")] // Explicitly maps to SQL table
    public class File
    {
        [Key]        
        public int FileID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(200)]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(50)]
        [Display(Name = "File Friendly Name")]
        public string FileFriendlyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(200)]
        [Display(Name = "File Type")]
        public string FileType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [Column("cqFile")]
        public byte[] FileObject { get; set; } 

        public int AgreementID { get; set; }
        public int CustomerID { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}

