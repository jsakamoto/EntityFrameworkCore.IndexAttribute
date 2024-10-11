using Toolbelt;
using Xunit;
using static Toolbelt.Diagnostics.XProcess;

namespace EFCore.IndexAttribute.Test.MigrationTest;

public class MigrationTest
{
#if NET6_0
    private const string Framework = "net6.0";
    private const string EFToolVersion = "6.0.*";
#elif NET8_0
    private const string Framework = "net8.0";
    private const string EFToolVersion = "8.0.*";
#endif

    [Fact]
    public async Task AddMigration_Initial_Test()
    {
        var testDir = FileIO.FindContainerDirToAncestor("*.csproj");
        using var workDir = WorkDirectory.CreateCopyFrom(Path.Combine(testDir, "TestProject"), _ => true);

        // Check the project is fine for build before adding migration.
        var build = await Start("dotnet", "build -f " + Framework + " --nologo", workDir).WaitForExitAsync();
        build.ExitCode.Is(n => n == 0, message: build.Output);

        var restoreTools = await Start("dotnet", "tool install dotnet-ef --version " + EFToolVersion, workDir).WaitForExitAsync();
        restoreTools.ExitCode.Is(n => n == 0, message: build.Output);

        // Add migration code, and...
        var migration = await Start("dotnet", "ef migrations add Initial --framework " + Framework, workDir).WaitForExitAsync();
        migration.ExitCode.Is(n => n == 0, message: build.Output);

        // Build it, and check it will be succeeded as expectedly.
        var rebuild = await Start("dotnet", "build -f " + Framework + " --nologo", workDir).WaitForExitAsync();
        rebuild.ExitCode.Is(n => n == 0, message: build.Output);
    }
}
