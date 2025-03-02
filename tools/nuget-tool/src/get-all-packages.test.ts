import {
  getAllPackages,
  writeResultsToFile,
  parseXmlFile,
  sortByAlphanumericallyByPackageName,
} from "./get-all-packages";
import { expect } from "chai";
import sinon from "sinon";
import fs from "fs";
import path from "path";
import { MockDirent } from "./types";

describe("getAllPackages", () => {
  let readdirStub: sinon.SinonStub;
  let readFileStub: sinon.SinonStub;
  let writeFileStub: sinon.SinonStub;
  const baseDir = "/test/dir";
  const outfile = "/test/output.xml";

  beforeEach(() => {
    sinon.restore();
    readdirStub = sinon.stub(fs.promises, "readdir");
    readFileStub = sinon.stub(fs.promises, "readFile");
    writeFileStub = sinon.stub(fs.promises, "writeFile");
  });

  afterEach(() => {
    sinon.restore();
  });

  it("should extract and write NuGet packages to a file", async () => {
    const mockStructure = new Map<string, MockDirent[]>([
      [baseDir, [new MockDirent("project1", false, true)]],
      [
        path.join(baseDir, "project1"),
        [new MockDirent("project1.csproj", true, false)],
      ],
    ]);

    const mockCsprojContent = `
      <Project>
        <ItemGroup>
          <PackageReference Include="Newtonsoft.Json" Version="12.0.3" />
          <PackageReference Include="NUnit" Version="3.12.0" />
        </ItemGroup>
      </Project>
    `;

    readdirStub.callsFake(async (dir) => {
      const entries = mockStructure.get(dir as string);
      if (!entries) throw new Error(`Unexpected directory: ${dir}`);
      return entries;
    });

    readFileStub.resolves(mockCsprojContent);

    await getAllPackages(baseDir, outfile);

    expect(writeFileStub.calledOnce).to.be.true;
    const writtenContent = writeFileStub.firstCall.args[1];
    expect(writtenContent).to.include(
      '<PackageVersion Include="Newtonsoft.Json" Version="12.0.3" />'
    );
    expect(writtenContent).to.include(
      '<PackageVersion Include="NUnit" Version="3.12.0" />'
    );
  });

  it("should handle empty directories and write an empty file", async () => {
    const mockStructure = new Map<string, MockDirent[]>([
      [baseDir, [new MockDirent("empty", false, true)]],
      [path.join(baseDir, "empty"), []],
    ]);

    readdirStub.callsFake(async (dir) => {
      const entries = mockStructure.get(dir as string);
      if (!entries) throw new Error(`Unexpected directory: ${dir}`);
      return entries;
    });

    await getAllPackages(baseDir, outfile);

    expect(writeFileStub.calledOnce).to.be.true;
    const writtenContent = writeFileStub.firstCall.args[1];
    expect(writtenContent).to.equal("<ItemGroup>\n</ItemGroup>");
  });

  it("should handle errors during directory reading", async () => {
    readdirStub.rejects(new Error("Permission denied"));

    try {
      await getAllPackages(baseDir, outfile);
      expect.fail("Expected an error to be thrown");
    } catch (error) {
      expect(error).to.be.an("error");
      // @ts-ignore
      expect(error.message).to.equal("Permission denied");
    }
  });
});
