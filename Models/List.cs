using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqLists")] // Explicitly maps to SQL table
    public class List
    {
        [Key]
        public int ListID { get; set; }      

        [Required(ErrorMessage = "Required")]
        [MaxLength(500)]
        [Display(Name = "List Name")]
        public string ListName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(500)]
        [Display(Name = "List Value")]
        public string ListValue { get; set; } = string.Empty;
    }
}

