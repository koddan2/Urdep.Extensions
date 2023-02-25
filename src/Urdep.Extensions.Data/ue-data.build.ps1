param(
    $Configuration = "Release",
    $ArtefactDir
)

task Format-Source {
    dotnet tool run dotnet-csharpier .
}

task Test Format-Source, {
    dotnet test
}

task Restore {
    dotnet restore
}

task Build Test, {
    dotnet build `
        --nologo `
        --configuration $Configuration
}

task Pack Test, {
    dotnet pack `
        --nologo `
        --output $ArtefactDir `
        --include-symbols `
        --include-source `
        --configuration $Configuration
}

task Clean {
    remove bin, obj, .vs
}

task . Format-Source