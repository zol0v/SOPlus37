namespace SOPlus37.Models
{
    public class AdminChargeResultItem
    {
        public int SubscriberID { get; set; }
        public string PhoneNumber { get; set; }

        public decimal TariffFee { get; set; }
        public decimal ServicesFee { get; set; }
        public decimal TotalFee => TariffFee + ServicesFee;

        public decimal BalanceAfter { get; set; }
        public string StatusAfter { get; set; }
    }
}
