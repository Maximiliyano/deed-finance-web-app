namespace Deed.Domain.Constants;

public static class ValidationConstants
{
    public const decimal ZeroValue = 0;

    public const int MaxLengthName = 32;

    public const int MaxLengthTagName = 75;

    public const int MaxLengthPurpose = 255;

    public const int MaxLengthUnit = 16;

    public const decimal MaxUnitPrice = 1_000_000;

    public const decimal MaxMeterReading = 1_000_000_000_000;

    public const string ErrorIsRequired = "The error instance is required";
}
