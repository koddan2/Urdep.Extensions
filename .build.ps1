#Requires -Version 7.0

param(
    [string]$Configuration = "Release",
    [string]$ArtefactDir = ".build",
    [int]$ThrottleLimit = 8
)

$allCsProj = Get-ChildItem ./**/**/*.csproj

task Format-Source {
    exec {
        $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
            #Action that will run in Parallel. Reference the current object via $PSItem and bring in outside variables with $USING:varname
            $dir = [System.IO.Path]::GetDirectoryName($PSItem)
            Push-Location $dir
            Invoke-Build Format-Source
        }
    }
}

task Test {
    exec {
        $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
            $dir = [System.IO.Path]::GetDirectoryName($PSItem)
            Push-Location $dir
            Invoke-Build Test
        }
    }
}

task Restore {
    exec {
        $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
            $dir = [System.IO.Path]::GetDirectoryName($PSItem)
            Push-Location $dir
            Invoke-Build Restore
        }
    }
}

task Build {
    exec {
        $allCsProj | Foreach-Object -ThrottleLimit $ThrottleLimit -Parallel {
            $dir = [System.IO.Path]::GetDirectoryName($PSItem)
            Push-Location $dir
            Invoke-Build Build -Configuration $USING:Configuration -ArtefactDir $USING:ArtefactDir
        }
    }
}

task Pack {
    exec {
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
    }
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