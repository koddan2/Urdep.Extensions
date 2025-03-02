import { parseStringPromise } from "xml2js";
import fs from "fs";
import { findCsprojFiles } from "@/common";
import { Nuget } from "@/types";

/**
 * This function enumerates all the .csproj files in the given directory (recursively) and
 * extracts the list of NuGet packages that are referenced by the projects, and writes
 * them to a file.
 * @param rootDir the root directory to start the search from
 * @param outfile the file to write the results to
 * @returns a promise that resolves when all the packages have been extracted and written to the outfile
 */
export async function getAllPackages(
  rootDir: string,
  outfile: string
): Promise<void> {
  // enumerate all .csproj files in the rootDir and descendant directories
  // and parse all <PackageReference> elements to determine the list of
  // NuGet packages that are referenced by the projects.
  const nugetPackages: Nuget[] = [];
  const csprojFiles = await findCsprojFiles(rootDir);
  for (const csprojFile of csprojFiles) {
    const doc = await parseXmlFile(csprojFile);
    if (doc.Project.ItemGroup)
      for (const itemGroup of Object.values(doc.Project.ItemGroup)) {
        if ((itemGroup as any).PackageReference) {
          for (const packageReference of Object.values(
            (itemGroup as any).PackageReference
          )) {
            const packageName = (packageReference as any).$.Include;
            const packageVersion = (packageReference as any).$.Version;
            if (packageName && packageVersion && packageVersion !== "undefined")
              nugetPackages.push({ packageName, packageVersion });
          }
        }
      }
  }

  sortByAlphanumericallyByPackageName(nugetPackages);
  const distinctPackages =
    filterPackagesSuchThatTheAreDisctinctByPackageName(nugetPackages);

  // implement filterPackagesSuchThatTheAreDisctinctByPackageName
  function filterPackagesSuchThatTheAreDisctinctByPackageName(
    packages: Nuget[]
  ) {
    const uniquePackages: Nuget[] = [];
    const seen = new Set();
    for (const pkg of packages) {
      if (!seen.has(pkg.packageName)) {
        seen.add(pkg.packageName);
        uniquePackages.push(pkg);
      } else {
        // compare the versions of the packages and take the latest
        const existingPackage = uniquePackages.find(
          (p) => p.packageName === pkg.packageName
        );
        if (
          existingPackage &&
          existingPackage.packageVersion < pkg.packageVersion
        ) {
          existingPackage.packageVersion = pkg.packageVersion;
        }
      }
    }
    return uniquePackages;
  }

  writeResultsToFile(outfile, distinctPackages);
}

export async function writeResultsToFile(
  outfile: string,
  nugetPackages: Nuget[]
) {
  const output = [
    "<ItemGroup>",
    ...nugetPackages.map(
      (pkg) =>
        `<PackageVersion Include="${pkg.packageName}" Version="${pkg.packageVersion}" />`
    ),
    "</ItemGroup>",
  ];
  await fs.promises.writeFile(outfile, output.join("\n"));
}

export async function parseXmlFile(filePath: string) {
  const xml = await fs.promises.readFile(filePath, "utf8");
  const doc = await parseStringPromise(xml);
  return doc;
}

export function sortByAlphanumericallyByPackageName(packages: Nuget[]) {
  packages.sort((a, b) => a.packageName.localeCompare(b.packageName));
}
