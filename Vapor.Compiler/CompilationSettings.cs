using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vapor.Compiler
{
    public class CompilationSettings
    {
        public string InputPath { get; set; } = "";
        public string OutputPath { get; set; } = "";
        public string AssemblyPath { get; set; } = "";
        public uint StackSize { get; set;} = 4000;
    }
}
