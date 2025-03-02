import fs from "fs";
import { JSDOM } from "jsdom";
import prettier from "prettier";
import prettierPluginXml from "@prettier/plugin-xml";

const xmlFilePath = process.argv[2];

// Read the XML file
async function sortNugetPackages() {
  try {
    const xmlBuffer = await fs.promises.readFile(xmlFilePath);
    const xml = xmlBuffer.toString("utf-8");
    const dom = new JSDOM(xml, { contentType: "text/xml" });
    const doc = dom.window.document;

    // Find all ItemGroup elements
    const itemGroups = doc.querySelectorAll("ItemGroup");

    for (const itemGroup of itemGroups) {
      // Get all child nodes with Include attributes (like PackageReference, ProjectReference, etc.)
      const children = Array.from(itemGroup.children).filter((node) =>
        node.hasAttribute("Include"),
      );

      if (children.length <= 1) continue; // No need to sort if there's only one or zero elements

      // Sort the children by Include attribute value (case-insensitive)
      children.sort((a, b) => {
        const aInclude = a.getAttribute("Include")?.toLowerCase() || "";
        const bInclude = b.getAttribute("Include")?.toLowerCase() || "";
        return aInclude.localeCompare(bInclude);
      });

      // Remove all children from the ItemGroup
      while (itemGroup.firstChild) {
        itemGroup.removeChild(itemGroup.firstChild);
      }

      // Add the sorted children back
      for (const child of children) {
        itemGroup.appendChild(child);
      }
    }

    let textToSave = doc.documentElement.outerHTML;

    // Check for XML declaration and BOM
    const hasBOM = xmlBuffer.length > xml.length;

    // Get the first line to check for XML declaration
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
        xmlFilePath,
        Buffer.concat([bomBuffer, contentBuffer]),
      );
    } else {
      // pretty print the XML using prettier and the plugin for XML
      textToSave = await prettier.format(textToSave, {
        parser: "xml",
        plugins: [prettierPluginXml],
        bracketSameLine: true,
        xmlWhitespaceSensitivity: "ignore",
      });
      await fs.promises.writeFile(xmlFilePath, textToSave);
    }

    console.log(`Package references in ${xmlFilePath} have been sorted.`);
  } catch (error) {
    console.error(`Error processing ${xmlFilePath}:`, error);
  }
}

sortNugetPackages().catch((err) => {
  console.error("Fatal error:", err);
  process.exit(1);
});
