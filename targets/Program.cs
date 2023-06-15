using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using static Bullseye.Targets;
using static SimpleExec.Command;

/*
See example at https://github.com/IdentityModel/IdentityModel/tree/main/build
*/

namespace targets
{
    internal static class Program
    {
        internal static class Dirs
        {
            public static string GetSourceFilePathName(
                [CallerFilePath] string? callerFilePath = null)
                => callerFilePath
                ?? throw new InvalidOperationException(nameof(callerFilePath));

            public static string Containing => Path.GetDirectoryName(GetSourceFilePathName())
                ?? throw new InvalidOperationException("Unable to get directory");

            public static string Parent => Path.GetDirectoryName(Containing)
                ?? throw new InvalidOperationException("Unable to get directory");

            public static string Src => Path.Combine(Parent, "src");
            public static string PackOutput => Path.Combine(Parent, "artifacts");
        }

        private const string _EnvVarMissing = " environment variable is missing. Aborting.";

        private static class TargetNames
        {
            public const string RestoreTools = "restore-tools";
            public const string CleanBuildOutput = "clean-build-output";
            public const string CleanPackOutput = "clean-pack-output";
            public const string Build = "build";
            public const string BuildDebug = "build-debug";
            public const string Test = "test";
            public const string Pack = "pack";
            public const string SignBinary = "sign-binary";
            public const string SignPackage = "sign-package";
            public const string FormatSource = "format-source";
        }

        private static class CsProj
        {
            private static string PathToCsProj(string name) => Path.Combine(Dirs.Src, name, $"{name}.csproj");
            public static string UrdepExtensionsAugmentation = PathToCsProj("Urdep.Extensions.Augmentation");
            public static string UrdepExtensionsData = PathToCsProj("Urdep.Extensions.Data");
            public static string UrdepExtensionsFileSystem = PathToCsProj("Urdep.Extensions.FileSystem");
            public static string UrdepExtensionsText = PathToCsProj("Urdep.Extensions.Text");

            public static IEnumerable<string> All()
            {
                yield return UrdepExtensionsAugmentation;
                yield return UrdepExtensionsData;
                yield return UrdepExtensionsFileSystem;
                yield return UrdepExtensionsText;
            }
        }

        static async Task Main(string[] args)
        {
            Target(TargetNames.RestoreTools, () =>
            {
                Run("dotnet", "tool restore");
            });

            Target(TargetNames.FormatSource, DependsOn(TargetNames.RestoreTools), async () =>
            {
                var tasks = CsProj.All().Select(csproj =>
                    RunAsync("dotnet", $"tool run dotnet-csharpier {Path.GetDirectoryName(csproj) ?? throw new InvalidOperationException()}")
                );
                await Task.WhenAll(tasks);
            });

            Target(TargetNames.CleanBuildOutput, async () =>
            {
                var tasks = CsProj.All().Select(csproj =>
                    RunAsync("dotnet", $"clean {csproj} -c Release -v m --nologo")
                );
                await Task.WhenAll(tasks);
            });

            Target(TargetNames.Build, DependsOn(TargetNames.CleanBuildOutput), async () =>
            {
                var tasks = CsProj.All().Select(csproj =>
                    RunAsync("dotnet", $"build {csproj} -c Release --nologo")
                );
                await Task.WhenAll(tasks);
            });

            Target(TargetNames.BuildDebug, DependsOn(TargetNames.CleanBuildOutput), async () =>
            {
                var tasks = CsProj.All().Select(csproj =>
                    RunAsync("dotnet", $"build {csproj} -c Debug --nologo")
                );
                await Task.WhenAll(tasks);
            });

            Target(TargetNames.Test, DependsOn(TargetNames.BuildDebug), async () =>
            {
                var tasks = CsProj.All().Select(csproj =>
                    RunAsync("dotnet", $"test {csproj} -c Debug --no-build --nologo")
                );
                await Task.WhenAll(tasks);
            });

            Target(TargetNames.CleanPackOutput, () =>
            {
                if (Directory.Exists(Dirs.PackOutput))
                {
                    Directory.Delete(Dirs.PackOutput, true);
                }
            });

            Target(TargetNames.Pack, DependsOn(TargetNames.Build, TargetNames.CleanPackOutput), async () =>
            {
                Directory.CreateDirectory(Dirs.PackOutput);

                var tasks = CsProj.All().Select(csproj =>
                    RunAsync("dotnet", $"pack {csproj} -c Release -o {Dirs.PackOutput} --no-build --nologo --include-symbols --include-source")
                );
                await Task.WhenAll(tasks);
            });

            ////Target(Targets.SignPackage, DependsOn(Targets.Pack, Targets.RestoreTools), () =>
            ////{
            ////    SignNuGet();
            ////});

            Target("default", DependsOn(TargetNames.Test, TargetNames.Pack));

            ////Target("sign", DependsOn(Targets.Test, Targets.SignPackage));

            await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException || ex.Message.EndsWith(_EnvVarMissing));
        }

        ////private static void SignNuGet()
        ////{
        ////    var signClientSecret = Environment.GetEnvironmentVariable("SignClientSecret");

        ////    if (string.IsNullOrWhiteSpace(signClientSecret))
        ////    {
        ////        throw new Exception($"SignClientSecret{envVarMissing}");
        ////    }

        ////    foreach (var file in Directory.GetFiles(packOutput, "*.nupkg", SearchOption.AllDirectories))
        ////    {
        ////        Console.WriteLine($"  Signing {file}");

        ////        Run("dotnet",
        ////                "NuGetKeyVaultSignTool " +
        ////                $"sign {file} " +
        ////                "--file-digest sha256 " +
        ////                "--timestamp-rfc3161 http://timestamp.digicert.com " +
        ////                "--azure-key-vault-url https://duendecodesigning.vault.azure.net/ " +
        ////                "--azure-key-vault-client-id 18e3de68-2556-4345-8076-a46fad79e474 " +
        ////                "--azure-key-vault-tenant-id ed3089f0-5401-4758-90eb-066124e2d907 " +
        ////                $"--azure-key-vault-client-secret {signClientSecret} " +
        ////                "--azure-key-vault-certificate CodeSigning"
        ////                , noEcho: true);
        ////    }
        ////}
    }
}
