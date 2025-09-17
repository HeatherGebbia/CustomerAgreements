using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CustomerAgreements.Validation;

namespace CustomerAgreements.Models
{
    [Table("cqSections")] // Explicitly maps to SQL table
    public class Section
    {
        [Key]
        public int SectionID { get; set; }

        [Required]
        public int QuestionnaireID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Column("Section")]
        [Display(Name = "Section")]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [Display(Name = "Include Instructions")]
        public bool IncludeInstructions { get; set; }

        [MaxLength(4000)]
        [RequiredIfIncludeInstructions(ErrorMessage = "Please provide instructions when Include Instructions = Yes.")]
        public string? Instructions { get; set; }

        // Navigation property
        public ICollection<Question>? Questions { get; set; }
    }
}

