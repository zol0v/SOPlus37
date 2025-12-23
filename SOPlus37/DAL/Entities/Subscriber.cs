using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("Subscriber")]
    public class Subscriber
    {
        [Key]
        public int SubscriberID { get; set; }

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal Balance { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public int TariffID { get; set; }

        public int? IndividualID { get; set; }
        public int? LegalEntityID { get; set; }

        [ForeignKey(nameof(IndividualID))]
        public Individual Individual { get; set; }

        [ForeignKey(nameof(LegalEntityID))]
        public LegalEntity LegalEntity { get; set; }

        [NotMapped]
        public bool IsLegalEntity => LegalEntityID.HasValue && !IndividualID.HasValue;
    }
}
