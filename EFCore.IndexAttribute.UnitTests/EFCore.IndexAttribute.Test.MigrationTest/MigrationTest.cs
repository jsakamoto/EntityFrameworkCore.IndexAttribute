using System;
using System.IO;
using Xunit;
using static EFCore.IndexAttribute.Test.MigrationTest.Internals.Shell;

namespace EFCore.IndexAttribute.Test.MigrationTest
{
    public class MigrationTest : IDisposable
    {
        private string TestProjDir { get; }

        public MigrationTest()
        {
            var testProjSrcDir = Path.Combine(GetTestDir(), "TestProject");
            TestProjDir = Environment.ExpandEnvironmentVariables(Path.Combine("%TEMP%", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(TestProjDir);
            Run(testProjSrcDir, "xcopy", "/S",
                $"\"{Path.Combine(testProjSrcDir, "*.*")}\"",
                $"\"{Path.Combine(TestProjDir, "*.*")}\"");
        }

        [Fact]
        public void AddMigration_Initial_Test()
        {
            // Check the project is fine for build before adding migration.
            Run(TestProjDir, "dotnet", "build").ExitCode.Is(0);

            // Add migration code, and...
            Run(TestProjDir, "dotnet", "ef", "migrations", "add", "initial").ExitCode.Is(0);

            // Build it, and check it will be succeeded as expectedly.
            Run(TestProjDir, "dotnet", "build").ExitCode.Is(0);
        }

        private static string GetTestDir()
        {
            var testDir = AppDomain.CurrentDomain.BaseDirectory;
            do { testDir = Path.GetDirectoryName(testDir); } while (!Exists(testDir, "*.sln"));
            testDir = Path.Combine(testDir, "EFCore.IndexAttribute.UnitTests", "EFCore.IndexAttribute.Test.MigrationTest");
            return testDir;
        }

        public void Dispose()
        {
            try { Delete(TestProjDir); } catch { }
        }
    }
}
