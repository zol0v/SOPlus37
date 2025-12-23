using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("Sms")]
    public class Sms
    {
        [Key]
        public int SmsID { get; set; }

        public int SubscriberID { get; set; }

        [Required]
        [MaxLength(15)]
        public string SmsedNumber { get; set; }

        public DateTime SmsDateTime { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }

        public bool IsPaid { get; set; }
    }
}
