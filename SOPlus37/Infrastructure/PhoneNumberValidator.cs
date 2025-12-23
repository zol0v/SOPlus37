using System.Text.RegularExpressions;

namespace SOPlus37.Infrastructure
{
    public static class PhoneNumberValidator
    {
        private static readonly Regex PhoneRegex = new Regex(@"^\+\d{11,}$", RegexOptions.Compiled);

        public static bool IsValid(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            phone = phone.Trim();
            return PhoneRegex.IsMatch(phone);
        }

        public static string Normalize(string phone)
        {
            return (phone ?? string.Empty).Trim();
        }
    }
}
