using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqCustomers")] // Explicitly maps to SQL table
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(128)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; } = string.Empty;
    }
}

