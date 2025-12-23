using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("Administrator")]
    public class Administrator
    {
        [Key]
        public int AdminID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }

        public bool IsActive { get; set; }
    }
}
