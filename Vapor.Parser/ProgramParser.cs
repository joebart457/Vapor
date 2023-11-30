using ParserLite.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore;
using TokenizerCore.Interfaces;
using TokenizerCore.Models.Constants;
using Vapor.Parser.Models;
using Vapor.Tokenizer;
using Vapor.Tokenizer.Constants;

namespace Vapor.Parser
{
    public class ProgramParser : ParserLite.TokenParser
    {
        private readonly NumberFormatInfo DefaultNumberFormat = new NumberFormatInfo { NegativeSign = "-" };
        private Dictionary<string, Int64> _equates = new Dictionary<string, long>();
        private List<byte> _compiledInstructions = new();
        private string _strings = "";
        private List<string> _originalStrings = new();
        private List<ParsedInstruction> _parsedInstructions = new();
        private const long MagicNumber = 0x039A;
        public ParsingResult ParseFile(string path)
        {
            _equates.Clear();
            _compiledInstructions.Clear();
            _strings = "";
            var tokenizer = Tokenizers.Default;
            var tokens = tokenizer.Tokenize(File.ReadAllText(path), false)
                .Where(token => token.Type != BuiltinTokenTypes.EndOfFile)
                .ToList();
            Initialize(tokens);

            while (!AtEnd())
            {
                var ins = ParseInstruction();
                if (ins != null)
                {
                    _parsedInstructions.Add(ins);
                    _compiledInstructions.AddRange(ins.GetInstructionByteCode());
                }
            }
            return new ParsingResult(_originalStrings, _parsedInstructions, GetFullyCompiledBytes());
        }
        

        private List<Byte> GetFullyCompiledBytes()
        {
            var bytes = new List<Byte>();
            bytes.AddRange(BitConverter.GetBytes(MagicNumber));
            bytes.AddRange(BitConverter.GetBytes((long)(16 + _strings.Length)));
            bytes.AddRange(Encoding.ASCII.GetBytes(_strings));
            bytes.AddRange(_compiledInstructions);
            return bytes;
        }

        private ParsedInstruction? ParseInstruction()
        {
            if (AdvanceIfMatch(TokenTypes.Push)) return ParsePush();
            if (AdvanceIfMatch(TokenTypes.Pushs)) return ParsePushs();
            if (AdvanceIfMatch(TokenTypes.Call)) return ParseCall();
            if (AdvanceIfMatch(BuiltinTokenTypes.Word))
            {
                ParseEquate();
                return null;
            }
            throw new ParsingException(Current(), "Expect instruction");
        }

        private ParsedInstruction ParsePush()
        {
            var token = Previous();
            var value = ParseLiteral();
            return new PushInstruction(token, value);
        }

        private ParsedInstruction ParsePushs()
        {
            var token = Previous();
            var value = ParseLiteral();
            return new PushsInstruction(token, value);
        }

        private ParsedInstruction ParseCall()
        {
            var token = Previous();
            var value = ParseLiteral();
            return new CallInstruction(token, value);
        }

        private void ParseEquate()
        {
            var symbol = Previous();
            AdvanceIfMatch(TokenTypes.Equal);
            var value = ParseLiteral(true);
            _equates[symbol.Lexeme] = value;
        }

        private Int64 Macro(IToken name)
        {
            if (_equates.TryGetValue(name.Lexeme, out var result)) return result;
            throw new ParsingException(name, $"unable to resolve symbol {name.Lexeme}");
        }

        private long ParseLiteral(bool allowCurrentPointer = false)
        {
            if (AdvanceIfMatch(BuiltinTokenTypes.Word))
            {
                return Macro(Previous());
            }
            if (allowCurrentPointer && AdvanceIfMatch(TokenTypes.Current))
            {
                return _compiledInstructions.Count;
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.Integer))
            {
                var previousLexeme = Previous().Lexeme;
                return Int64.Parse(previousLexeme, DefaultNumberFormat);
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.Double))
            {              
                return BitConverter.DoubleToInt64Bits(double.Parse(Previous().Lexeme, DefaultNumberFormat));
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.Float))
            {
                return BitConverter.DoubleToInt64Bits(double.Parse(Previous().Lexeme, DefaultNumberFormat));
            }
            if (AdvanceIfMatch(BuiltinTokenTypes.UnsignedInteger))
            {
                var previousLexeme = Previous().Lexeme;
                return Int64.Parse(previousLexeme, DefaultNumberFormat);
            }
           
            if (AdvanceIfMatch(BuiltinTokenTypes.String))
            {
                var result = _strings.Length + 8 + 8;
                _strings += Previous().Lexeme;
                _strings += (char)0;
                _originalStrings.Add(Previous().Lexeme);
                return result;
            }

            throw new ParsingException(Current(), $"encountered unexpected token {Current()}");
        }
    }
}
