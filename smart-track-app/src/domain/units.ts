// src/domain/unit.ts

import { Unit } from "../services/clients/api-client";

export const allUnits = Object.values(Unit) as Unit[];

export function unitLabel(u: Unit): string {
  switch (u) {
    case Unit.None:
      return "";
    case Unit.Part:
      return "Qty";
    case Unit.Gram:
      return "Gram (g)";
    case Unit.Litre:
      return "Litre (l)";
    case Unit.Milliliter:
      return "Milliliter (ml)";
    case Unit.Cup:
      return "Cups";
    case Unit.Tablespoon:
      return "Tablespoon (tbsp)";
    case Unit.Teaspoon:
      return "Teaspoon (tsp)";
    case Unit.Ounce:
      return "Ounces (oz)";
    case Unit.Pound:
      return "Pounds (lb)";
    case Unit.Kilogram:
      return "Kilograms (kg)";
    case Unit.Gallon:
      return "Gallons (gal)";
    case Unit.Quart:
      return "Quarts (qt)";
    case Unit.Pint:
      return "Pints (pt)";
    case Unit.FluidOunce:
      return "Fluid Ounces (fl oz)";
    case Unit.CubicCentimeter:
      return "Cubic Centimeters (cc)";
    case Unit.CubicMeter:
      return "Cubic Meters (m³)";
    case Unit.CubicInch:
      return "Cubic Inch (in³)";
    default:
      return String(u);
  }
}

export function isUnit(x: unknown): x is Unit {
  return allUnits.includes(x as Unit);
}

export function formatQty(qty?: number, unit?: unknown) {
  if (qty == null) return "-";
  return isUnit(unit) ? `${qty} ${unitLabel(unit)}` : `${qty}`;
}
