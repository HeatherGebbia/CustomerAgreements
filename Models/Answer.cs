using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqAnswers")] // Explicitly maps to SQL table
    public class Answer
    {
        [Key]
        public int AnswerID { get; set; }
        public int AgreementID { get; set; }  
        public int QuestionnaireID { get; set; }
        public int SectionID { get; set; }
        public int QuestionID { get; set; }
        public int QuestionListID { get; set; }        

        [Required(ErrorMessage = "Required")]
        [MaxLength(8000)]
        [Column("Answer")] // Keeps mapping to DB column named "Question"
        public string Text { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "datetime2")]
        public DateTime? DateAnswer { get; set; }

        [ForeignKey("AgreementID")]
        public Agreement? Agreement { get; set; }

        [ForeignKey("QuestionID")]
        public Question? Question { get; set; }

        // Navigation property 
        public ICollection<DependentAnswer> DependentAnswers { get; set; } = new List<DependentAnswer>();
    }
}

