import { parseStringPromise } from "xml2js";
import fs from "fs";
import { findCsprojFiles } from "./common";

// get the first argument passed from the command line and treat it as a path to the root directory
const rootDir = process.argv[2];
const outfile = process.argv[3];

type Nuget = {
  packageName: string;
  packageVersion: string;
};

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

// console.log(csprojFiles);
// console.log(nugetPackages);

sortByAlphanumericallyByPackageName(nugetPackages);
const distinctPackages =
  filterPackagesSuchThatTheAreDisctinctByPackageName(nugetPackages);

// implement filterPackagesSuchThatTheAreDisctinctByPackageName
function filterPackagesSuchThatTheAreDisctinctByPackageName(packages) {
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

// implement writeResultsToFile
async function writeResultsToFile(outfile, nugetPackages) {
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

// implement findCsprojFiles

// implement parseXmlFile using xml2js's parseStringPromise
async function parseXmlFile(filePath) {
  const xml = await fs.promises.readFile(filePath, "utf8");
  const doc = await parseStringPromise(xml);
  return doc;
}

function sortByAlphanumericallyByPackageName(packages) {
  packages.sort((a, b) => a.packageName.localeCompare(b.packageName));
}
