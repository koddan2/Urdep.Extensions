param($Task)

Import-Module $PSScriptRoot/InvokeBuild.psm1

Invoke-Build -Task $Task
Invoke-Build -Task Restore