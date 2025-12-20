using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqDependentQuestions")] // Explicitly maps to SQL table
    public class DependentQuestion
    {
        [Key]
        public int DependentQuestionID { get; set; }
        public int QuestionnaireID { get; set; }
        public int SectionID { get; set; }
        public int QuestionID { get; set; }
        public int QuestionListID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Display(Name = "Dependent Question Title")]
        public string DependentQuestionTitle { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Required")]
        //[MaxLength(8000)]
        [Column("DependentQuestion")]
        [Display(Name = "Dependent Question")]
        public string? Text { get; set; } 

        [Required(ErrorMessage = "Required")]
        [MaxLength(4000)]
        [Display(Name = "Dependent Question Text")]
        public string DependentQuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(50)]
        [Display(Name = "Dependent Answer Type")]
        public string DependentAnswerType { get; set; } = string.Empty;

        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        // Navigation property 
        public ICollection<DependentQuestionList> DependentQuestionLists { get; set; } = new List<DependentQuestionList>();
    }
}

