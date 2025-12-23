namespace SOPlus37.Models
{
    public class AdminServiceItem
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }
        public decimal Cost { get; set; }
        public bool IsRecurring { get; set; }
        public string Description { get; set; }
        public int CreatedByAdminID { get; set; }
    }
}
