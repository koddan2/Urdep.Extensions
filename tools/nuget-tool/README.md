
# Workflow:

```bash
# \nuget-tool (compute all distinct PackageVersion elements from the .csproj files found in the directory)
> node --import=tsx .\get-all-packages.ts ...\Directory.packages.props
# \nuget-tool (remove all Version attributes in the .csproj files)
> node --import=tsx .\update-csproj-files.ts ...\
>
```

Example

```bash
node --import=tsx .\sort-nuget-packages.ts ...\Directory.Packages.props
```