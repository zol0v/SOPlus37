using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("LegalEntity")]
    public class LegalEntity
    {
        [Key]
        public int LegalEntityID { get; set; }

        [Required]
        [MaxLength(100)]
        public string OrganizationName { get; set; }

        [Required]
        [MaxLength(13)]
        public string OGRN { get; set; }
    }
}
