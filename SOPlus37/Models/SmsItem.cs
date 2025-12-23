using System;

namespace SOPlus37.Models
{
    public class SmsItem
    {
        public string SmsedNumber { get; set; }
        public DateTime SmsDateTime { get; set; }
        public decimal Cost { get; set; }
        public bool IsPaid { get; set; }

        public string PaidText => IsPaid ? "Оплачено" : "Не оплачено";
    }
}
