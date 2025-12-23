using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOPlus37.DAL.Entities
{
    [Table("SubscriberService")]
    public class SubscriberService
    {
        [Key]
        public int SubscriberServiceID { get; set; }

        public int SubscriberID { get; set; }

        public int ServiceID { get; set; }

        public bool IsActive { get; set; }

        public DateTime? ActivatedDate { get; set; }
    }
}
