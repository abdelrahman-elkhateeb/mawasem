using System.Text.RegularExpressions;

namespace Mawasem.Infrastructure.Authentication;

internal static partial class EgyptianPhoneNumberNormalizer
{
    public static bool TryNormalize(
        string? phoneNumber ,
        out string normalizedPhoneNumber )
    {
        normalizedPhoneNumber = string.Empty;

        if ( string.IsNullOrWhiteSpace(phoneNumber) )
        {
            return false;
        }

        var value = phoneNumber
            .Trim()
            .Replace(" " , string.Empty)
            .Replace("-" , string.Empty)
            .Replace("(" , string.Empty)
            .Replace(")" , string.Empty);

        if ( value.StartsWith("0020" , StringComparison.Ordinal) )
        {
            value = $"+20{value[4..]}";
        }
        else if ( value.StartsWith("20" , StringComparison.Ordinal) )
        {
            value = $"+{value}";
        }
        else if ( value.StartsWith('0') )
        {
            value = $"+20{value[1..]}";
        }

        if ( !EgyptianMobileNumberRegex().IsMatch(value) )
        {
            return false;
        }

        normalizedPhoneNumber = value;

        return true;
    }

    [GeneratedRegex(
        @"^\+201[0125]\d{8}$" ,
        RegexOptions.CultureInvariant)]
    private static partial Regex EgyptianMobileNumberRegex();
}