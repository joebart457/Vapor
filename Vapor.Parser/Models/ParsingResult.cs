using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vapor.Parser.Models
{
    public class ParsingResult
    {

        public List<string> Strings { get; private set; }
        public List<ParsedInstruction> Instructions { get; private set; }
        public List<Byte> CompiledBytes { get; private set; }
        public ParsingResult(List<string> strings, List<ParsedInstruction> instructions, List<byte> compiledBytes)
        {
            Strings = strings;
            Instructions = instructions;
            CompiledBytes = compiledBytes;
        }


    }
}
