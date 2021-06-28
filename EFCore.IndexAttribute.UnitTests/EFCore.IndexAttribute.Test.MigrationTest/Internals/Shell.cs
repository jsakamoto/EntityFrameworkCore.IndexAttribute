﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.IndexAttribute.Test.MigrationTest.Internals
{
    public static class Shell
    {
        public static bool Exists(string dir, string wildCard)
        {
            return Directory.GetFiles(dir, wildCard, SearchOption.TopDirectoryOnly).Any();
        }

        public static void Delete(string dir)
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }

        public static Process Run(string workDir, params string[] args)
        {
            var pi = new ProcessStartInfo
            {
                WorkingDirectory = workDir,
                FileName = args.First(),
                Arguments = string.Join(" ", args.Skip(1)),
                UseShellExecute = false,
            };
            var process = Process.Start(pi);
            process.WaitForExit();
            return process;
        }
    }
}
