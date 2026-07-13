namespace Mawasem.Application.Features.Authentication.Options;

public sealed class SuperAdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullNameAr { get; set; } = string.Empty;

    public string FullNameEn { get; set; } = string.Empty;

    public bool HasAnyValue()
    {
        return !string.IsNullOrWhiteSpace(Email) ||
               !string.IsNullOrWhiteSpace(Password) ||
               !string.IsNullOrWhiteSpace(FullNameAr) ||
               !string.IsNullOrWhiteSpace(FullNameEn);
    }

    public bool IsComplete()
    {
        return !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(Password) &&
               !string.IsNullOrWhiteSpace(FullNameAr) &&
               !string.IsNullOrWhiteSpace(FullNameEn);
    }
}