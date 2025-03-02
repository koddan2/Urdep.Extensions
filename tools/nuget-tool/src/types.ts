export type Nuget = {
  packageName: string;
  packageVersion: string;
};

// MockDirent class for testing purposes
export class MockDirent {
  name: string;
  private _isFile: boolean;
  private _isDirectory: boolean;

  constructor(name: string, isFile: boolean, isDirectory: boolean) {
    this.name = name;
    this._isFile = isFile;
    this._isDirectory = isDirectory;
  }

  isFile(): boolean {
    return this._isFile;
  }

  isDirectory(): boolean {
    return this._isDirectory;
  }
}
