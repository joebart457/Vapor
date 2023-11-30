using CliParser;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vapor.Compiler;

namespace Vapor.Cmd
{
    [Entry("vaporc")]
    internal class StartupService
    {
        [Command]
        public int Compile(
            [Option("inputPath", "i", "path to the file that will be compiled. (Ignored if projectPath is provided)")] string inputPath,
            [Option("outputPath", "o", "path to the final generated binary")] string? outputPath = null,
            [Option("assemblyOutputPath", "a", "path to output internally produced assembly. If omitted, a temporary file will be used")] string? assemblyOutputPath = null)
        {
            try
            {
                var settings = new CompilationSettings();
                settings.InputPath = inputPath;
                settings.AssemblyPath = assemblyOutputPath ?? Path.GetTempFileName();
                settings.OutputPath = outputPath ?? string.Empty;
               
                var compiler = new ProgramCompiler();
                var err = compiler.Compile(settings);
                if (err == null)
                {
                    if (settings.AssemblyPath != null) CliLogger.LogSuccess($"{inputPath} => {settings.AssemblyPath}");
                    CliLogger.LogSuccess($"{inputPath} => {settings.OutputPath}");
                    return 0;
                }
                CliLogger.LogWarning(err);
                return 1;
            }
            catch (Exception ex)
            {
                CliLogger.LogError($"fatal error: {ex.Message}");
                return 2;
            }
        }
    }
}
