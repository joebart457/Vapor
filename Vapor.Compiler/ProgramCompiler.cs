using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vapor.Compiler.Services;
using Vapor.Parser;
using Vapor.Parser.Models;

namespace Vapor.Compiler
{
    public class ProgramCompiler
    {


        public string? Compile(CompilationSettings settings)
        {
            var parser = new ProgramParser();
            var result = parser.ParseFile(settings.InputPath);
            SantizeAssemblyFilePath(settings);
            SantizeOutputFilePath(settings);
            OutputResult(result, settings);
            return FasmService.RunFasm(settings);
        }

        private static string SantizeAssemblyFilePath(CompilationSettings settings)
        {
            var outputPath = Path.GetFullPath(settings.AssemblyPath);
            if (Path.GetExtension(outputPath) != ".asm") outputPath = $"{outputPath}.asm";
            settings.AssemblyPath = outputPath;
            return outputPath;
        }

        private static string SantizeOutputFilePath(CompilationSettings settings)
        {
            if (string.IsNullOrEmpty(settings.OutputPath)) settings.OutputPath = Path.GetFullPath(Path.GetFileNameWithoutExtension(settings.InputPath));
            var outputPath = Path.GetFullPath(settings.OutputPath);
            outputPath = $"{outputPath}.exe";
            settings.OutputPath = outputPath;
            return outputPath;
        }


        private void OutputResult(ParsingResult result, CompilationSettings settings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("format PE64 console");
            sb.AppendLine("entry _start");
            sb.AppendLine($"STACK_SIZE equ {settings.StackSize}");
            sb.AppendLine();
            sb.AppendLine("section '.data' data readable writeable");
            sb.AppendLine($"    _code      db {FormatBytes(result)}");
            sb.AppendLine("    _endcode    dq $");
            sb.AppendLine(File.ReadAllText($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Templates\\runtime.asm"));
            File.WriteAllText(settings.AssemblyPath, sb.ToString());
        }
        private static string FormatBytes(ParsingResult result)
        {        
            var strBytes = BitConverter.ToString(result.CompiledBytes.ToArray());
            return $"0x{strBytes.Replace("-", ",0x")}";
        }
    }
}
