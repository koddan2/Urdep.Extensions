import fs from "fs";
import path from "path";
import { mockable } from "@atcodes/mockable";

/**
 * Recursively find all .csproj files in the given directory
 * @param dir Directory to search in
 * @returns Array of absolute paths to .csproj files
 */
async function _findCsprojFiles(dir: string): Promise<string[]> {
  const results: string[] = [];

  async function scan(directory: string) {
    const entries = await fs.promises.readdir(directory, {
      withFileTypes: true,
    });

    for (const entry of entries) {
      const fullPath = path.join(directory, entry.name);

      if (entry.isDirectory()) {
        // Skip node_modules, .git and other special directories
        if ([".git", "node_modules", "bin", "obj"].includes(entry.name)) {
          continue;
        }
        await scan(fullPath);
      } else if (entry.isFile() && entry.name.endsWith(".csproj")) {
        results.push(fullPath);
      }
    }
  }

  await scan(dir);
  return results;
}

export const findCsprojFiles = mockable(_findCsprojFiles);
