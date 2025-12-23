using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("BalanceReplenishment")]
    public class BalanceReplenishment
    {
        [Key]
        public int ReplenishmentID { get; set; }

        public int SubscriberID { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; }

        public DateTime ReplenishmentDate { get; set; }
    }
}
