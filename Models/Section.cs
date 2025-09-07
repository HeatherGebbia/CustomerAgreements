using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqSections")] // Explicitly maps to SQL table
    public class Section
    {
        [Key]
        public int SectionID { get; set; }
        public int QuestionnaireID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Column("Section")]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [Display(Name = "Include Instructions")]
        public bool IncludeInstructions { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(4000)]
        public string Instructions { get; set; } = string.Empty;
    }
}

