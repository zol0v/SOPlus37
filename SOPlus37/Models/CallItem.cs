using System;

namespace SOPlus37.Models
{
    public class CallItem
    {
        public string CalledNumber { get; set; }
        public DateTime CallDateTime { get; set; }
        public int DurationMinutes { get; set; }

        public string CallTypeText { get; set; }
        public decimal Cost { get; set; }
    }
}
