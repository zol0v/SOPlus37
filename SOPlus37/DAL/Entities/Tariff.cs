using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("Tariff")]
    public class Tariff
    {
        [Key]
        public int TariffID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TariffName { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal CityCallCost { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal IntercityCallCost { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal InternationalCallCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SmsCost { get; set; }

        [Column("TariffSwitchCost", TypeName = "decimal(10,2)")]
        public decimal SwitchCost { get; set; }

        public bool IsActive { get; set; }

        public int CreatedByAdminID { get; set; }

        [NotMapped]
        public bool IsCorporate => (TariffName ?? "").StartsWith("Корпоратив", System.StringComparison.OrdinalIgnoreCase);
    }
}
