import { updateCsprojFiles } from "./update-csproj-files";
import { findCsprojFiles } from "./common";
import { expect } from "chai";
import sinon from "sinon";
import fs from "fs";
import path from "path";

describe("Update Csproj Files", () => {
  let findCsprojFilesFake: sinon.SinonSpy;
  let readFileStub: sinon.SinonStub;
  let writeFileStub: sinon.SinonStub;

  const testDir = "/test/dir";
  const testFiles = [
    path.join(testDir, "project1", "project1.csproj"),
    path.join(testDir, "project2", "project2.csproj"),
  ];

  beforeEach(() => {
    // Restore any previous stubs
    sinon.restore();

    // Create stubs for the functions we need to mock
    readFileStub = sinon.stub(fs.promises, "readFile");
    writeFileStub = sinon.stub(fs.promises, "writeFile").resolves();
  });

  afterEach(() => {
    sinon.restore();
  });

  it("should remove Version attribute from PackageReference elements", async () => {
    // Setup
    const xmlContent = `<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Package1" Version="1.0.0" />
    <PackageReference Include="Package2" Version="2.3.4" />
  </ItemGroup>
</Project>`;

    // Expected XML after transformation (without Version attributes)
    const expectedXml = `<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Package1"></PackageReference>
    <PackageReference Include="Package2"></PackageReference>
  </ItemGroup>
</Project>`;

    const findCsprojFilesFake = sinon.fake(async (s) => [testFiles[0]]);
    findCsprojFiles.override!(findCsprojFilesFake);
    readFileStub.resolves(Buffer.from(xmlContent, "utf8"));

    // Act
    await updateCsprojFiles(testDir);

    // Assert
    expect(findCsprojFilesFake.calledWith(testDir)).to.be.true;
    expect(readFileStub.calledWith(testFiles[0])).to.be.true;

    // Check if writeFile was called with the correct arguments
    // Note: The exact XML formatting might differ slightly due to JSDOM serialization
    const writeFileCall = writeFileStub.getCall(0);
    expect(writeFileCall).to.not.be.null;
    expect(writeFileCall.args[0]).to.equal(testFiles[0]);

    // Check that Version attributes are removed in the written content
    const writtenContent = writeFileCall.args[1].toString();
    expect(writtenContent).to.not.include('Version="');
    expect(writtenContent).to.include('PackageReference Include="Package1"');
    expect(writtenContent).to.include('PackageReference Include="Package2"');
  });

  it("should handle files with BOM correctly", async () => {
    // Setup XML with BOM
    const xmlContent = `<?xml version="1.0" encoding="utf-8"?>
<Project>
  <ItemGroup>
    <PackageReference Include="Package1" Version="1.0.0" />
  </ItemGroup>
</Project>`;

    // Create a buffer with BOM
    const bomBuffer = Buffer.from([0xef, 0xbb, 0xbf]);
    const contentBuffer = Buffer.from(xmlContent, "utf8");
    const fileBuffer = Buffer.concat([bomBuffer, contentBuffer]);

    const findCsprojFilesFake = sinon.fake(async (s) => [testFiles[0]]);
    findCsprojFiles.override!(findCsprojFilesFake);
    readFileStub.resolves(fileBuffer);

    // Act
    await updateCsprojFiles(testDir);

    // Assert
    expect(readFileStub.calledWith(testFiles[0])).to.be.true;

    // Check if writeFile was called with a buffer that includes BOM
    const writeFileCall = writeFileStub.getCall(0);
    expect(writeFileCall).to.not.be.null;

    // Verify the buffer starts with BOM bytes
    const writtenBuffer = writeFileCall.args[1];
    expect(Buffer.isBuffer(writtenBuffer)).to.be.true;
    expect(writtenBuffer[0]).to.equal(0xef);
    expect(writtenBuffer[1]).to.equal(0xbb);
    expect(writtenBuffer[2]).to.equal(0xbf);
  });

  it("should preserve XML declaration", async () => {
    // Setup XML with declaration
    const xmlContent = `<?xml version="1.0" encoding="utf-8"?>
<Project>
  <ItemGroup>
    <PackageReference Include="Package1" Version="1.0.0" />
  </ItemGroup>
</Project>`;

    const findCsprojFilesFake = sinon.fake(async (s) => [testFiles[0]]);
    findCsprojFiles.override!(findCsprojFilesFake);
    readFileStub.resolves(Buffer.from(xmlContent, "utf8"));

    // Act
    await updateCsprojFiles(testDir);

    // Assert
    const writeFileCall = writeFileStub.getCall(0);
    expect(writeFileCall).to.not.be.null;

    // Check that XML declaration is preserved
    const writtenContent = writeFileCall.args[1].toString();
    expect(writtenContent).to.include('<?xml version="1.0" encoding="utf-8"?>');
  });

  it("should process multiple csproj files", async () => {
    // Setup
    const xml1 = `<Project><ItemGroup><PackageReference Include="Package1" Version="1.0.0" /></ItemGroup></Project>`;
    const xml2 = `<Project><ItemGroup><PackageReference Include="Package2" Version="2.0.0" /></ItemGroup></Project>`;

    const findCsprojFilesFake = sinon.fake(async (s) => testFiles);
    findCsprojFiles.override!(findCsprojFilesFake);
    readFileStub.onFirstCall().resolves(Buffer.from(xml1, "utf8"));
    readFileStub.onSecondCall().resolves(Buffer.from(xml2, "utf8"));

    // Act
    await updateCsprojFiles(testDir);

    // Assert
    expect(readFileStub.calledWith(testFiles[0])).to.be.true;
    expect(readFileStub.calledWith(testFiles[1])).to.be.true;
    expect(writeFileStub.callCount).to.equal(2);
  });

  it("should handle files with no PackageReference elements", async () => {
    // Setup XML without PackageReference
    const xmlContent = `<Project>
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
  </PropertyGroup>
</Project>`;

    const findCsprojFilesFake = sinon.fake(async (s) => [testFiles[0]]);
    findCsprojFiles.override!(findCsprojFilesFake);
    readFileStub.resolves(Buffer.from(xmlContent, "utf8"));

    // Act
    await updateCsprojFiles(testDir);

    // Assert
    const writeFileCall = writeFileStub.getCall(0);
    expect(writeFileCall).to.not.be.null;

    // Check content is still properly written
    const writtenContent = writeFileCall.args[1].toString();
    expect(writtenContent).to.include(
      "<TargetFramework>net7.0</TargetFramework>"
    );
  });

  it("should throw an error if file reading fails", async () => {
    // Setup
    const findCsprojFilesFake = sinon.fake(async (s) => [testFiles[0]]);
    findCsprojFiles.override!(findCsprojFilesFake);
    readFileStub.rejects(new Error("File read error"));

    // Act and Assert
    try {
      await updateCsprojFiles(testDir);
      expect.fail("Expected an error to be thrown");
    } catch (error) {
      expect(error).to.be.an("error");
      // @ts-ignore
      expect(error.message).to.equal("File read error");
    }
  });
});
