param(
    $Configuration = "Release",
    $ArtefactDir = "obj"
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
        --configuration $Configuration
}

task . Format-Source