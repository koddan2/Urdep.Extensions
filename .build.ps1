#Requires -Version 7.0

param(
    [string]$Configuration = "Release",
    [string]$ArtefactDir = ".build",
    [int]$ThrottleLimit = 8
)

$allCsProj = Get-ChildItem ./**/**/*.csproj

task Format-Source {
    $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        #Action that will run in Parallel. Reference the current object via $PSItem and bring in outside variables with $USING:varname
        $dir = [System.IO.Path]::GetDirectoryName($PSItem)
        Push-Location $dir
        Invoke-Build Format-Source
    }
}

task Test Format-Source, {
    $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        $dir = [System.IO.Path]::GetDirectoryName($PSItem)
        Push-Location $dir
        Invoke-Build Test
    }
}

task Restore {
    $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        $dir = [System.IO.Path]::GetDirectoryName($PSItem)
        Push-Location $dir
        Invoke-Build Restore
    }
}

task Build Test, {
    $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        $dir = [System.IO.Path]::GetDirectoryName($PSItem)
        Push-Location $dir
        Invoke-Build Build
    }
}

task Pack Test, {
    $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
        $dir = [System.IO.Path]::GetDirectoryName($PSItem)
        Push-Location $dir
        Invoke-Build Pack
    }
}

task . Format-Source