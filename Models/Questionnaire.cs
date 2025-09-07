using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqQuestionnaires")] // Explicitly maps to SQL table
    public class Questionnaire
    {
        [Key]
        public int QuestionnaireID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(50)]
        [Display(Name = "Questionnaire Name")]
        public string QuestionnaireName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property (EF will use this later when we add Questions)
        public ICollection<Question>? Questions { get; set; }
    }
}
