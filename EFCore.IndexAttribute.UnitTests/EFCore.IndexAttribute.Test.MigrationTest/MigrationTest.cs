using Toolbelt;
using static Toolbelt.Diagnostics.XProcess;

namespace EFCore.IndexAttribute.Test.MigrationTest;

public class MigrationTest
{
    [Test]
    public async Task AddMigration_Initial_Test()
    {
        var testDir = FileIO.FindContainerDirToAncestor("*.csproj");
        using var workDir = WorkDirectory.CreateCopyFrom(Path.Combine(testDir, "TestProject"), _ => true);

        // Check the project is fine for build before adding migration.
        var build = await Start("dotnet", "build --nologo", workDir).WaitForExitAsync();
        build.ExitCode.Is(0, message: build.Output);

        // Add migration code, and...
        //Run(this.TestProjDir, "dotnet", "ef", "migrations", "add", "initial").ExitCode.Is(0);
        var migration = await Start("dotnet", "ef migrations add initial", workDir).WaitForExitAsync();
        migration.ExitCode.Is(0, message: build.Output);

        // Build it, and check it will be succeeded as expectedly.
        var rebuild = await Start("dotnet", "build --nologo", workDir).WaitForExitAsync();
        rebuild.ExitCode.Is(0, message: build.Output);
    }
}
