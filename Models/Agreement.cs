using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqAgreements")] // Explicitly maps to SQL table
    public class Agreement
    {
        [Key]
        public int AgreementID { get; set; }

        public int CustomerID { get; set; }  
        public int QuestionnaireID { get; set; }

        [MaxLength(128)]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Customer Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public bool ReadPrivacyPolicy { get; set; }
        public DateTime? SubmittedDate { get; set; }

        [MaxLength(120)]
        public string SubmittedByName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public DateTime? ArchivedDate { get; set; }

        [MaxLength(1500)]
        public string ArchivedReason { get; set; } = string.Empty;


        // Navigation property (optional: lets EF link back to parent questionnaire)
        [ForeignKey("QuestionnaireID")]
        public Questionnaire? Questionnaire { get; set; }

        [ForeignKey("CustomerID")]
        public Customer? Customer { get; set; }

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        
    }
}

