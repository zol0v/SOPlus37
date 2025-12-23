namespace SOPlus37.Models
{
    public class AdminSubscriberItem
    {
        public int SubscriberID { get; set; }

        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public decimal Balance { get; set; }

        public string ClientType { get; set; }
        public string NameOrOrg { get; set; }
        public string Doc { get; set; } // Паспорт или ОГРН

        public string TariffName { get; set; }

        public int CallsCount { get; set; }
        public int SmsCount { get; set; }

        public int ServicesCount { get; set; }
    }
}
