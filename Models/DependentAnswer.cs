using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqDependentAnswers")] // Explicitly maps to SQL table
    public class DependentAnswer
    {
        [Key]
        public int DependentAnswerID { get; set; }
        public int AgreementID { get; set; }  
        public int QuestionnaireID { get; set; }
        public int SectionID { get; set; }
        public int QuestionID { get; set; }
        public int QuestionListID { get; set; }
        public int DependentQuestionID { get; set; }
        public int DependentQuestionListID { get; set; }
        public int AnswerID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(4000)]
        public string Answer { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "datetime2")]
        public DateTime? DateAnswer { get; set; }

    }
}

