
# TrackingCopyTool

Tries to be smart about what to copy.

## Usage

### Example 1

Copy all files that
- match `c:/my/dir/somewhere/**/*.*` 
- but not those that match `c:/my/dir/somewhere/**/*.dump`
- to `//maybe/c$/some/unc` (creating the target dir(s) if they do not exist.)

```powershell
TrackingCopyTool.exe -d c:/my/dir/somewhere --includes:0 **/*.* --excludes:0 **/*.dump --target:name //maybe/some/unc --target:create true
```

----

Passing `--force true` to the invocation will
a. create directories that does not exist
b. copy all files, disregarding the target manifest

```powershell
TrackingCopyTool.exe -d c:/my/dir/somewhere --includes:0 **/*.* --target:name c:/target-dir --force true
```
