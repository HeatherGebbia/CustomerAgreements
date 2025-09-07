using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqDependentQuestionLists")] // Explicitly maps to SQL table
    public class DependentQuestionList
    {
        [Key]
        public int DependentQuestionListID { get; set; }
        public int QuestionnaireID { get; set; }
        public int SectionID { get; set; }
        public int QuestionID { get; set; }
        public int DependentQuestionID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(500)]
        [Display(Name = "List Value")]
        public string ListValue { get; set; } = string.Empty;

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }
}

