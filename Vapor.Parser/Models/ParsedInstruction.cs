using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vapor.Parser.Models
{
    public abstract class ParsedInstruction
    {
        public abstract byte[] GetInstructionByteCode();
    }
}
