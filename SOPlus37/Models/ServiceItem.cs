using System;

namespace SOPlus37.Models
{
    public class ServiceItem
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public bool IsRecurring { get; set; }

        public bool IsConnected { get; set; }
        public DateTime? ActivatedDate { get; set; }

        public string TypeText
        {
            get
            {
                switch ((ServiceType ?? "").ToUpperInvariant())
                {
                    case "INTERNET": return "Интернет";
                    case "OTHER": return "Другое";
                    default: return ServiceType ?? "";
                }
            }
        }
    }
}
