param(
    $Configuration = "Release",
    $ArtefactDir
)

task Format-Source {
    exec { dotnet tool run dotnet-csharpier . }
}

task Test Format-Source, {
    exec { dotnet test }
}

task Restore {
    exec { dotnet restore }
}

task Build Test, {
    exec {
        dotnet build `
            --nologo `
            --configuration $Configuration
    }
}

task Pack Test, {
    exec {
        dotnet pack `
            --nologo `
            --output $ArtefactDir `
            --include-symbols `
            --include-source `
            --configuration $Configuration
    }
}

task Clean {
    exec {
        remove bin, obj, .vs
    }
}

task . Format-Source