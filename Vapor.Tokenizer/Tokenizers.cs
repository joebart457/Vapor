using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Model;
using TokenizerCore.Models.Constants;
using TokenizerCore;
using Vapor.Tokenizer.Constants;

namespace Vapor.Tokenizer
{
    public static class Tokenizers
    {
        private static List<TokenizerRule> _defaultRules => new List<TokenizerRule>()
        {
                    new TokenizerRule(BuiltinTokenTypes.EndOfLineComment, "//"),
                    new TokenizerRule(TokenTypes.Push, "push"),
                    new TokenizerRule(TokenTypes.Pushs, "pushs"),
                    new TokenizerRule(TokenTypes.Call, "call"),
                    new TokenizerRule(TokenTypes.Equal, "="),
                    new TokenizerRule(TokenTypes.Equal, "equ"),
                    new TokenizerRule(BuiltinTokenTypes.String, "\"", enclosingLeft: "\"", enclosingRight: "\""),
                    new TokenizerRule(BuiltinTokenTypes.String, "'", enclosingLeft: "'", enclosingRight: "'"),
        };
        public static TokenizerSettings DefaultSettings => new TokenizerSettings
        {
            AllowNegatives = true,
            NegativeChar = '-',

        };
        public static TokenizerCore.Tokenizer Default => new TokenizerCore.Tokenizer(_defaultRules, DefaultSettings);
    }
}
