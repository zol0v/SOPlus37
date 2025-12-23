namespace SOPlus37.Models
{
    public class FilterOption
    {
        public string Value { get; set; }
        public string Display { get; set; }

        public override string ToString() => Display;
    }
}
