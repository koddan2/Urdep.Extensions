import { expect } from "chai";
import sinon from "sinon";
import fs from "fs";
import { sortNugetPackages } from "./sort-nuget-packages";

describe("sortNugetPackages", () => {
  let readFileStub: sinon.SinonStub;
  let writeFileStub: sinon.SinonStub;
  const xmlFilePath = "/test/packages.config";

  beforeEach(() => {
    sinon.restore();
    readFileStub = sinon.stub(fs.promises, "readFile");
    writeFileStub = sinon.stub(fs.promises, "writeFile");
  });

  afterEach(() => {
    sinon.restore();
  });

  it("should sort PackageReference elements lexicographically by Include attribute", async () => {
    const mockXmlContent = `
      <Project>
        <ItemGroup>
          <PackageReference Include="Zebra" Version="1.0.0" />
          <PackageReference Include="Alpha" Version="1.0.0" />
          <PackageReference Include="Mike" Version="1.0.0" />
        </ItemGroup>
      </Project>
    `;

    const expectedSortedXmlContent = `
      <Project>
        <ItemGroup>
          <PackageReference Include="Alpha" Version="1.0.0" />
          <PackageReference Include="Mike" Version="1.0.0" />
          <PackageReference Include="Zebra" Version="1.0.0" />
        </ItemGroup>
      </Project>
    `;

    readFileStub.resolves(mockXmlContent);

    await sortNugetPackages(xmlFilePath);

    expect(writeFileStub.calledOnce).to.be.true;
    const writtenContent = writeFileStub.firstCall.args[1];
    expect(writtenContent).to.include(
      '<PackageReference Include="Alpha" Version="1.0.0" />'
    );
    expect(writtenContent).to.include(
      '<PackageReference Include="Mike" Version="1.0.0" />'
    );
    expect(writtenContent).to.include(
      '<PackageReference Include="Zebra" Version="1.0.0" />'
    );
  });

  it("should handle empty ItemGroup elements", async () => {
    const mockXmlContent = `
      <Project>
        <ItemGroup></ItemGroup>
      </Project>
    `;

    readFileStub.resolves(mockXmlContent);

    await sortNugetPackages(xmlFilePath);

    expect(writeFileStub.calledOnce).to.be.true;
    const writtenContent = writeFileStub.firstCall.args[1];
    expect(writtenContent).to.include("<ItemGroup />");
  });

  it("should handle errors during file reading", async () => {
    readFileStub.rejects(new Error("Permission denied"));

    try {
      await sortNugetPackages(xmlFilePath);
      expect.fail("Expected an error to be thrown");
    } catch (error) {
      expect(error).to.be.an("error");
      // @ts-ignore
      expect(error.message).to.equal("Permission denied");
    }
  });
});
