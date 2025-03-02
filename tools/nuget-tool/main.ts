import { getAllPackages } from "@/get-all-packages";
import { sortNugetPackages } from "@/sort-nuget-packages";
import { updateCsprojFiles } from "@/update-csproj-files";
import * as fs from "fs";
import * as path from "path";

const argv = process.argv;
const node = argv[0];
const script = argv[1];
const subCommand = argv[2];

const rest = argv.slice(3);

// console.log(node, script, subCommand);

// Dispatch subCommand to the appropriate function.
// The allowed subCommands are:
// - 'extract-packages'
//   Extracts all package information from all .csproj files in the given root directory and its subdirectories
// - 'remove-version-attributes'
//   Removes all version attributes from all .csproj files in the given root directory and its subdirectories
// - 'sort-packages'
//   Sorts all package references in the given MSBuild compatible XML file.
switch (subCommand) {
  case "extract-packages":
    validateDirExists(rest[0]);
    validateDirExists(path.dirname(rest[1]));
    validateFileDoesNotExist(rest[1]);
    getAllPackages(rest[0], rest[1]);
    break;
  case "remove-version-attributes":
    validateDirExists(rest[0]);
    updateCsprojFiles(rest[0]);
    break;
  case "sort-packages":
    validateFileExists(rest[0]);
    validateFileSeemsToBeXml(rest[0]);
    sortNugetPackages(rest[0]);
    break;
  default:
    console.error(`Unknown sub-command: ${subCommand}`);
    break;
}

function validateDirExists(dirPath: string) {
  if (!fs.existsSync(dirPath)) {
    console.error(`Directory does not exist: ${dirPath}`);
    process.exit(1);
  }
  if (!fs.statSync(dirPath).isDirectory()) {
    console.error(`Path is not a directory: ${dirPath}`);
    process.exit(1);
  }
}

function validateFileExists(filePath: string) {
  if (!fs.existsSync(filePath)) {
    console.error(`File does not exist: ${filePath}`);
    process.exit(1);
  }
}

function validateFileDoesNotExist(filePath: string) {
  if (fs.existsSync(filePath)) {
    console.error(`File already exists: ${filePath}`);
    process.exit(1);
  }
}

function validateFileSeemsToBeXml(filePath: string) {
  const fileContent = fs.readFileSync(filePath, "utf8");
  if (!fileContent.trim().startsWith("<?xml")) {
    console.error(`File does not seem to be a valid XML: ${filePath}`);
    process.exit(1);
  }
}
