namespace SOPlus37.Models
{
    public class TariffItem
    {
        public int TariffID { get; set; }
        public string TariffName { get; set; }
        public decimal SwitchCost { get; set; }
        public decimal CityCallCost { get; set; }
        public decimal IntercityCallCost { get; set; }
        public decimal InternationalCallCost { get; set; }
        public decimal SmsCost { get; set; }
        public bool IsCorporate { get; set; }

        public string TypeText => IsCorporate ? "Корпоративный" : "Некорпоративный";
    }
}
