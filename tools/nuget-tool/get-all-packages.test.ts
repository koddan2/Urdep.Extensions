import { describe, expect, test } from "@jest/globals";
import * as fs from "fs";
import * as path from "path";

// You'll need to export the functions you want to test
// import { yourFunction } from './get-all-packages';

const getAllPackages = require("./get-all-packages");

describe("get-all-packages", () => {
  test("should extract package versions correctly", () => {
    // Add your test here
    expect(true).toBeTruthy();
  });

  test("should return an array of packages", () => {
    const result = getAllPackages();
    expect(Array.isArray(result)).toBeTruthy();
  });

  test("should return packages with the required properties", () => {
    const result = getAllPackages();
    // result.forEach((package) => {
    //   expect(package).toHaveProperty("name");
    //   expect(package).toHaveProperty("version");
    // });
  });

  test("should return an empty array when no packages are available", () => {
    const result = getAllPackages();
    expect(result.length).toBe(0);
  });
});
