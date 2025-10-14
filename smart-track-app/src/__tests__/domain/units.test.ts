import { allUnits, unitLabel, isUnit, formatQty } from "../../domain/units";
import { Unit } from "../../services/clients/api-client";

describe("units helpers", () => {
  test("allUnits contains known enums", () => {
    expect(allUnits).toContain(Unit.Gram);
    expect(allUnits).toContain(Unit.Litre);
  });

  test("unit labels", () => {
    expect(unitLabel(Unit.Gram)).toMatch(/Gram/);
    expect(unitLabel(Unit.Teaspoon)).toMatch(/Teaspoon/);
  });

  test("isUnit type guard", () => {
    expect(isUnit(Unit.Pound)).toBe(true);
    expect(isUnit("not-a-unit")).toBe(false);
  });

  test("format quantity", () => {
    expect(formatQty(undefined, Unit.Gram)).toBe("-");
    expect(formatQty(3, Unit.Cup)).toMatch(/3 .*Cups/);
    expect(formatQty(5)).toBe("5");
  });
});
