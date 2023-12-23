
# CleverCopy

Tries to be smart about what to copy.

## Usage

### Example 1

Copy all files that
- match `c:/my/dir/somewhere/**/*.*` 
- but not those that match `c:/my/dir/somewhere/**/*.dump`
- to `//maybe/c$/some/unc` (creating the target dir(s) if they do not exist.)

```powershell
CleverCopy.exe --SourceDirectory=c:/my/dir/somewhere --IncludeGlobs:0 **/*.* --ExcludeGlobs:0 **/*.dump --TargetDirectory //maybe/some/unc
```

Pass `--Verbosity=X` to control how much stuff is reported. Ranges from -1 to 5, where lower means less.

----
