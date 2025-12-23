using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("CallType")]
    public class CallType
    {
        [Key]
        public int CallTypeID { get; set; }

        [Required]
        [MaxLength(50)]
        public string TypeName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
    }
}
