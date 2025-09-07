using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerAgreements.Models
{
    [Table("cqNotifications")] // Explicitly maps to SQL table
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required(ErrorMessage = "Required")]
        [MaxLength(50)]
        [Display(Name = "Notification Name")]
        public string NotificationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(50)]
        [Display(Name = "Key")]
        public string NotificationKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(8000)]
        [Column("Notification")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(10)]
        [Display(Name = "Type")]
        public string NotificationType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Display(Name = "Send To")]
        public string SendTo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [MaxLength(100)]
        [Display(Name = "Send From")]
        public string SendFrom { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Send CC")]
        public string SendCC { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Send BCC")]
        public string SendBCC { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}

