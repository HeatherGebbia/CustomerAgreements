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
        public int QuestionnaireID { get; set; }
        public int SectionID { get; set; }
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(500)]
        [Display(Name = "List Value")]
        public string ListValue { get; set; } = string.Empty;
        public bool Conditional { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [Display(Name = "Send Email")]
        public bool SendEmail { get; set; }
        public int NotificationID { get; set; }
    }
}

