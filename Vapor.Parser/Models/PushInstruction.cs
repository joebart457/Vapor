using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Interfaces;

namespace Vapor.Parser.Models
{
    public class PushInstruction: ParsedInstruction
    {
        public IToken Token { get; private set; }
        public Int64 Value { get; private set; }
        public PushInstruction(IToken token, Int64 value)
        {
            Token = token;
            Value = value;
        }

        public override byte[] GetInstructionByteCode()
        {
            var bytes = new List<byte> { 0 };
            bytes.AddRange(BitConverter.GetBytes(Value));
            return bytes.ToArray();
        }
    }

    public class PushsInstruction : ParsedInstruction
    {
        public IToken Token { get; private set; }
        public Int64 Value { get; private set; }
        public PushsInstruction(IToken token, Int64 value)
        {
            Token = token;
            Value = value;
        }

        public override byte[] GetInstructionByteCode()
        {
            var bytes = new List<byte> { 1 };
            bytes.AddRange(BitConverter.GetBytes(Value));
            return bytes.ToArray();
        }
    }
}
