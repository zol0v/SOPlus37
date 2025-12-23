namespace SOPlus37.Models
{
    public class AdminTariffItem
    {
        public int TariffID { get; set; }
        public string TariffName { get; set; }

        public decimal CityCallCost { get; set; }
        public decimal IntercityCallCost { get; set; }
        public decimal InternationalCallCost { get; set; }
        public decimal SmsCost { get; set; }
        public decimal SwitchCost { get; set; }

        public bool IsActive { get; set; }
        public int CreatedByAdminID { get; set; }

        public string StatusText => IsActive ? "Активен" : "Неактивен";
    }
}
