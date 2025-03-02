import { findCsprojFiles } from "./common";
import fs from "fs";
import { JSDOM } from "jsdom";

const rootDir = process.argv[2];
const csprojFiles = await findCsprojFiles(rootDir);

for (const csprojFile of csprojFiles) {
  // using jsdom, parse the csprojfile
  const xmlBuffer = await fs.promises.readFile(csprojFile);
  const xml = xmlBuffer.toString("utf-8");
  const dom = new JSDOM(xml, { contentType: "text/xml" });
  const doc = dom.window.document;
  // find all <PackageReference> elements using the jsdom interface
  const packageReferences = doc.querySelectorAll("PackageReference");
  // iterate over all the packageReferences and update the elements by removing any Version attribute
  for (const packageReference of packageReferences) {
    packageReference.removeAttribute("Version");
  }

  let textToSave = doc.documentElement.outerHTML;

  // Check for XML declaration and BOM
  // Detect if the original file had a BOM by comparing the buffer length with the string length
  const hasBOM = xmlBuffer.length > xml.length;

  // get the first line of xml and test whether it is an xml declaration
  const firstLine = xml.split("\n")[0].trim();
  if (firstLine.startsWith("<?xml")) {
    textToSave = firstLine + "\n" + textToSave;
  }

  // Write the file back with the same encoding (with or without BOM)
  if (hasBOM) {
    // Add UTF-8 BOM (EF BB BF) to the beginning of the file
    const bomBuffer = Buffer.from([0xef, 0xbb, 0xbf]);
    const contentBuffer = Buffer.from(textToSave, "utf8");
    await fs.promises.writeFile(
      csprojFile,
      Buffer.concat([bomBuffer, contentBuffer])
    );
  } else {
    await fs.promises.writeFile(csprojFile, textToSave);
  }
}
