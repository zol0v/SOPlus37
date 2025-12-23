using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("Call")]
    public class Call
    {
        [Key]
        public int CallID { get; set; }

        public int SubscriberID { get; set; }

        [Required]
        [MaxLength(15)]
        public string CalledNumber { get; set; }

        public DateTime CallDateTime { get; set; }

        public int Duration { get; set; }

        public int CallTypeID { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost { get; set; }

        public bool IsPaid { get; set; }
    }
}
