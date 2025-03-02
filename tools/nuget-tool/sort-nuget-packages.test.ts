import { describe, expect, test } from "@jest/globals";
import * as fs from "fs";
import * as path from "path";
const { sortNugetPackages } = require("./sort-nuget-packages");

describe("sortNugetPackages", () => {
  test("should sort packages by version in ascending order", () => {
    const packages = [
      { name: "packageA", version: "1.0.0" },
      { name: "packageB", version: "0.9.0" },
      { name: "packageC", version: "1.1.0" },
    ];
    const sorted = sortNugetPackages(packages);
    expect(sorted).toEqual([
      { name: "packageB", version: "0.9.0" },
      { name: "packageA", version: "1.0.0" },
      { name: "packageC", version: "1.1.0" },
    ]);
  });

  test("should handle an empty array", () => {
    const sorted = sortNugetPackages([]);
    expect(sorted).toEqual([]);
  });

  test("should handle a single package", () => {
    const packages = [{ name: "packageA", version: "1.0.0" }];
    const sorted = sortNugetPackages(packages);
    expect(sorted).toEqual(packages);
  });

  test("should sort packages with the same version", () => {
    const packages = [
      { name: "packageB", version: "1.0.0" },
      { name: "packageA", version: "1.0.0" },
    ];
    const sorted = sortNugetPackages(packages);
    expect(sorted).toEqual([
      { name: "packageA", version: "1.0.0" },
      { name: "packageB", version: "1.0.0" },
    ]);
  });
});
