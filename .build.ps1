#Requires -Version 7.0

param(
    [string]$Configuration = "Release",
    [string]$ArtefactDir = ".build",
    [int]$ThrottleLimit = 8
)

$allCsProj = Get-ChildItem ./**/**/*.csproj

task Format-Source {
    # exec {
    $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        #Action that will run in Parallel. Reference the current object via $PSItem and bring in outside variables with $USING:varname
        $dir = [System.IO.Path]::GetDirectoryName($PSItem)
        Push-Location $dir
        Invoke-Build Format-Source
    }
    # }
}

task Test {
    # $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
    #     $dir = [System.IO.Path]::GetDirectoryName($PSItem)
    #     Push-Location $dir
    #     Invoke-Build Test
    # }
    $tasks = New-Object System.Collections.ArrayList
    foreach ($csproj in $allCsProj) {
        $dir = [System.IO.Path]::GetDirectoryName($csproj)
        $invocation = [hashtable]@{
            File = Get-ChildItem (Join-Path $dir "*.build.ps1")
            Task = "Test"
        }
        $tasks.Add($invocation)
    }
    write-host $tasks
    Build-Parallel $tasks
}

task Restore {
    # exec {
    # $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
    #     $dir = [System.IO.Path]::GetDirectoryName($PSItem)
    #     Push-Location $dir
    #     Invoke-Build Restore
    # }
    # }
    $tasks = New-Object System.Collections.ArrayList
    foreach ($csproj in $allCsProj) {
        $dir = [System.IO.Path]::GetDirectoryName($csproj)
        $invocation = [hashtable]@{
            File = Get-ChildItem (Join-Path $dir "*.build.ps1")
            Task = "Restore"
        }
        $tasks.Add($invocation)
    }
    write-host $tasks
    Build-Parallel $tasks
}

task Build {
    # exec {
    # $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
    #     $dir = [System.IO.Path]::GetDirectoryName($PSItem)
    #     Push-Location $dir
    #     Invoke-Build Build -Configuration $USING:Configuration -ArtefactDir $USING:ArtefactDir
    # }
    # }
    $tasks = New-Object System.Collections.ArrayList
    foreach ($csproj in $allCsProj) {
        $dir = [System.IO.Path]::GetDirectoryName($csproj)
        $invocation = [hashtable]@{
            File = Get-ChildItem (Join-Path $dir "*.build.ps1")
            Task = "Build"
        }
        $tasks.Add($invocation)
    }
    write-host $tasks
    Build-Parallel $tasks
}

task Pack {
    # exec {
    if (![System.IO.Path]::IsPathFullyQualified($ArtefactDir)) {
        $output = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ArtefactDir))
    }
    else {
        $output = $ArtefactDir
    }
    $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        $dir = [System.IO.Path]::GetDirectoryName($PSItem)
        Push-Location $dir
        Invoke-Build Pack `
            -File (Get-ChildItem *.build.ps1) `
            -Configuration $USING:Configuration `
            -ArtefactDir $USING:output
    }
    # }
}

task Clean {
    exec {
        $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
            $dir = [System.IO.Path]::GetDirectoryName($PSItem)
            Push-Location $dir
            Invoke-Build Clean
        }
        remove $ArtefactDir, src/.vs
    }
}

task . Format-Source