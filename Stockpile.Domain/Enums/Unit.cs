using System.Text.Json.Serialization;

namespace Stockpile.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<Unit>))]
public enum Unit
{
    None = 0,
    Part = 1,
    Gram = 2,
    Litre = 3,
    Milliliter = 4,
    Cup = 5,
    Tablespoon = 6,
    Teaspoon = 7,
    Ounce = 8,
    Pound = 9,
    Kilogram = 10,
    Gallon = 11,
    Quart = 12,
    Pint = 13,
    FluidOunce = 14,
    CubicCentimeter = 15,
    CubicMeter = 16,
    CubicInch = 17
}