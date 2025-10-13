using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqQuestionLists")] // Explicitly maps to SQL table
    public class QuestionList
    {
        [Key]
        public int QuestionListID { get; set; }
        [Required]
        public int QuestionnaireID { get; set; }
        [Required]
        public int SectionID { get; set; }
        [Required]
        public int QuestionID { get; set; }
        public Question? Question { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(500)]
        [Display(Name = "List Value")]
        public string ListValue { get; set; } = string.Empty;
        public bool Conditional { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        public bool SendEmail { get; set; }

        public int? NotificationID { get; set; }

        // Navigation property 
        public ICollection<DependentQuestion> DependentQuestions { get; set; } = new List<DependentQuestion>();
    }
}

