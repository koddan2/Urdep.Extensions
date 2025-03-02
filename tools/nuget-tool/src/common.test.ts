import { findCsprojFiles } from "./common";
import path from "path";
import { expect } from "chai";
import sinon from "sinon";
import fs from "fs";
import { MockDirent } from "./types";

describe("Common Functions", () => {
  describe("findCsprojFiles", () => {
    let readdirStub: sinon.SinonStub;
    const baseDir = "/test/dir";

    beforeEach(() => {
      // Restore any previous stubs
      sinon.restore();

      // Create stub for fs.promises.readdir
      readdirStub = sinon.stub(fs.promises, "readdir");
    });

    afterEach(() => {
      sinon.restore();
    });

    it("should find csproj files in a directory", async () => {
      // Mock filesystem structure
      const mockStructure = new Map<string, MockDirent[]>([
        [
          baseDir,
          [
            new MockDirent("project1", false, true),
            new MockDirent("file.txt", true, false),
          ],
        ],
        [
          path.join(baseDir, "project1"),
          [
            new MockDirent("project1.csproj", true, false),
            new MockDirent("Program.cs", true, false),
          ],
        ],
      ]);

      // Configure the stub to return appropriate data based on the directory being read
      readdirStub.callsFake(async (dir) => {
        const entries = mockStructure.get(dir as string);
        if (!entries) throw new Error(`Unexpected directory: ${dir}`);
        return entries;
      });

      const result = await findCsprojFiles(baseDir);

      expect(result).to.be.an("array").with.lengthOf(1);
      expect(result[0]).to.equal(
        path.join(baseDir, "project1", "project1.csproj")
      );
    });

    it("should skip specified directories", async () => {
      // Mock filesystem structure with directories that should be skipped
      const mockStructure = new Map<string, MockDirent[]>([
        [
          baseDir,
          [
            new MockDirent("project1", false, true),
            new MockDirent("node_modules", false, true),
            new MockDirent(".git", false, true),
          ],
        ],
        [
          path.join(baseDir, "project1"),
          [new MockDirent("project1.csproj", true, false)],
        ],
        [
          path.join(baseDir, "node_modules"),
          [
            new MockDirent("something.csproj", true, false), // Should be skipped
          ],
        ],
        [
          path.join(baseDir, ".git"),
          [
            new MockDirent("hidden.csproj", true, false), // Should be skipped
          ],
        ],
      ]);

      readdirStub.callsFake(async (dir) => {
        const entries = mockStructure.get(dir as string);
        if (!entries) throw new Error(`Unexpected directory: ${dir}`);
        return entries;
      });

      const result = await findCsprojFiles(baseDir);

      expect(result).to.be.an("array").with.lengthOf(1);
      expect(result[0]).to.equal(
        path.join(baseDir, "project1", "project1.csproj")
      );
      // Verify node_modules and .git were not accessed
      expect(readdirStub.calledWith(path.join(baseDir, "node_modules"))).to.be
        .false;
      expect(readdirStub.calledWith(path.join(baseDir, ".git"))).to.be.false;
    });

    it("should handle nested directory structures", async () => {
      // Mock a more complex nested structure
      const mockStructure = new Map<string, MockDirent[]>([
        [baseDir, [new MockDirent("src", false, true)]],
        [
          path.join(baseDir, "src"),
          [
            new MockDirent("ProjectA", false, true),
            new MockDirent("ProjectB", false, true),
            new MockDirent("solution.sln", true, false),
          ],
        ],
        [
          path.join(baseDir, "src", "ProjectA"),
          [
            new MockDirent("ProjectA.csproj", true, false),
            new MockDirent("bin", false, true),
          ],
        ],
        [
          path.join(baseDir, "src", "ProjectB"),
          [
            new MockDirent("ProjectB.csproj", true, false),
            new MockDirent("obj", false, true),
          ],
        ],
        [
          path.join(baseDir, "src", "ProjectA", "bin"),
          [
            new MockDirent("output.csproj", true, false), // Should be skipped
          ],
        ],
        [
          path.join(baseDir, "src", "ProjectB", "obj"),
          [
            new MockDirent("temp.csproj", true, false), // Should be skipped
          ],
        ],
      ]);

      readdirStub.callsFake(async (dir) => {
        const entries = mockStructure.get(dir as string);
        if (!entries) throw new Error(`Unexpected directory: ${dir}`);
        return entries;
      });

      const result = await findCsprojFiles(baseDir);

      expect(result).to.be.an("array").with.lengthOf(2);
      expect(result).to.include(
        path.join(baseDir, "src", "ProjectA", "ProjectA.csproj")
      );
      expect(result).to.include(
        path.join(baseDir, "src", "ProjectB", "ProjectB.csproj")
      );
      // Verify bin and obj were not accessed
      expect(
        readdirStub.calledWith(path.join(baseDir, "src", "ProjectA", "bin"))
      ).to.be.false;
      expect(
        readdirStub.calledWith(path.join(baseDir, "src", "ProjectB", "obj"))
      ).to.be.false;
    });

    it("should handle empty directories and return empty array when no csproj files found", async () => {
      const mockStructure = new Map<string, MockDirent[]>([
        [
          baseDir,
          [
            new MockDirent("empty", false, true),
            new MockDirent("text.txt", true, false),
          ],
        ],
        [path.join(baseDir, "empty"), []],
      ]);

      readdirStub.callsFake(async (dir) => {
        const entries = mockStructure.get(dir as string);
        if (!entries) throw new Error(`Unexpected directory: ${dir}`);
        return entries;
      });

      const result = await findCsprojFiles(baseDir);

      expect(result).to.be.an("array").with.lengthOf(0);
    });

    it("should handle errors during directory reading", async () => {
      readdirStub.rejects(new Error("Permission denied"));

      try {
        await findCsprojFiles(baseDir);
        expect.fail("Expected an error to be thrown");
      } catch (error) {
        expect(error).to.be.an("error");
        // @ts-ignore
        expect(error.message).to.equal("Permission denied");
      }
    });
  });
});
