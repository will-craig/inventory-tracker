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
      return "Ounce (oz)";
    case Unit.Pound:
      return "Pound (lb)";
    case Unit.Kilogram:
      return "Kilogram (kg)";
    case Unit.Gallon:
      return "Gallon (gal)";
    case Unit.Quart:
      return "Quart (qt)";
    case Unit.Pint:
      return "Pint (pt)";
    case Unit.FluidOunce:
      return "Fluid Ounce (fl oz)";
    case Unit.CubicCentimeter:
      return "Cubic Centimeter (cc)";
    case Unit.CubicMeter:
      return "Cubic Meter (m³)";
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
