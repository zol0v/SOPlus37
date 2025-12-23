namespace SOPlus37.Models
{
    public class AdminSubscribersStats
    {
        public int TotalSubscribers { get; set; }
        public int IndividualsCount { get; set; }
        public int LegalEntitiesCount { get; set; }
        public int ActiveCount { get; set; }
        public int BlockedCount { get; set; }

        public int TotalCalls { get; set; }
        public int TotalSms { get; set; }

        public decimal TotalBalance { get; set; }
    }
}
