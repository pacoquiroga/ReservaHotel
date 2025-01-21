using System.Text.RegularExpressions;

namespace ReservasHotel.Helpers
{
    public static partial class RegexHelpers
    {
        [GeneratedRegex(@"^\+?\d{10,15}$")]
        public static partial Regex PhoneRegex();
    }
}
