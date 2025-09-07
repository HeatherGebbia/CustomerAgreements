using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqQuestionLibrary")] // Explicitly maps to SQL table
    public class QuestionLibrary
    {
        [Key]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Display(Name = "Question Title")]
        public string QuestionTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Display(Name = "Question Key")]
        public string QuestionKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(8000)]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(4000)]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(50)]
        [Display(Name = "Answer Type")]
        public string AnswerType { get; set; } = string.Empty;

        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

