$settings = @{
    Foo        = 42
    ResultName = "result.xml"
}
.\invoke-pipeline.ps1 .\pipeline.ps1 -Verbose -InputValue ..\complex\source-file.sql -Settings $settings
