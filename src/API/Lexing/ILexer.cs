using Compilador.Common;
using Compilador.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Lexing
{
    public interface ILexer
    {
        Token TryLex(GrammarDefinition grammar, LineColumnMapping mapping, string source, ref int offset);
    }

    public class KeywordLexer : ILexer
    {
        public string Keyword { get; private set; }
        public KeywordLexer(string keyword)
        {
            this.Keyword = keyword;
        }
        public Token TryLex(GrammarDefinition grammar, LineColumnMapping mapping, string source, ref int offset)
        {
            for (int i = 0; i < Keyword.Length; i++)
            {
                if ((offset + i) >= source.Length)
                    return null;

                if (grammar.IsCaseInsensitive)
                {
                    if (char.ToLowerInvariant(source[offset + i]) != char.ToLowerInvariant(Keyword[i]) &&
                        char.ToUpperInvariant(source[offset + i]) != char.ToUpperInvariant(Keyword[i]))
                        return null;
                }
                else
                {
                    if (source[offset + i] != Keyword[i])
                        return null;
                }
            }

            var original = offset;
            offset += Keyword.Length;
            return new Token(Keyword, source.Substring(original, Keyword.Length), mapping.GetLocation(original));
        }
    }

    public class WhitespaceLexer : ILexer
    {
        public Token TryLex(GrammarDefinition grammar, LineColumnMapping mapping, string source, ref int offset)
        {
            if (char.IsWhiteSpace(source[offset]))
            {
                int original = offset;
                while (offset < source.Length && char.IsWhiteSpace(source[offset]))
                    offset++;

                return new Token(Token.WhitespaceName, source.Substring(original, offset - original), mapping.GetLocation(original));
            }
            else
            {
                var line_tk = TryLexKeyword(grammar.LineComment, grammar, mapping, source, ref offset);
                if(line_tk != null)
                {
                    int original = offset;
                    while (offset < source.Length && source[offset] != '\n')
                        offset++;

                    return new Token(Token.WhitespaceName, line_tk.Source + source.Substring(original, offset - original), line_tk.Location);
                }
                else
                {
                    var before_block_tk = offset;
                    var block_tk = TryLexKeyword(grammar.StartBlockComment, grammar, mapping, source, ref offset);
                    if (block_tk != null)
                    {
                        int original = offset;
                        while (offset < source.Length && !StartsWith(grammar, source, offset, grammar.EndBlockComment))
                            offset++;

                        if (offset >= source.Length)
                        {
                            offset = before_block_tk;
                            return null;
                        }
                        else
                        {
                            var tk = new Token(Token.WhitespaceName,
                                block_tk.Source + source.Substring(original, offset - original) + source.Substring(offset, grammar.EndBlockComment.Length),
                                block_tk.Location);
                            offset += grammar.EndBlockComment.Length;
                            return tk;
                        }
                    }
                    else
                        return null;
                }
            }
        }

        private bool StartsWith(GrammarDefinition grammar, string source, int offset, string substring)
        {
            for (int i = 0; i < substring.Length; i++)
            {
                if ((offset + i) >= source.Length)
                    return false;

                if (grammar.IsCaseInsensitive)
                {
                    if (char.ToLowerInvariant(source[offset + i]) != char.ToLowerInvariant(substring[i]) &&
                        char.ToUpperInvariant(source[offset + i]) != char.ToUpperInvariant(substring[i]))
                        return false;
                }
                else
                {
                    if (source[offset + i] != substring[i])
                        return false;
                }
            }

            return true;
        }

        private Token TryLexKeyword(string keyword, GrammarDefinition grammar, LineColumnMapping mapping, string source, ref int offset)
        {
            for (int i = 0; i < keyword.Length; i++)
            {
                if ((offset + i) >= source.Length)
                    return null;

                if (grammar.IsCaseInsensitive)
                {
                    if (char.ToLowerInvariant(source[offset + i]) != char.ToLowerInvariant(keyword[i]) &&
                        char.ToUpperInvariant(source[offset + i]) != char.ToUpperInvariant(keyword[i]))
                        return null;
                }
                else
                {
                    if (source[offset + i] != keyword[i])
                        return null;
                }
            }

            var original = offset;
            offset += keyword.Length;
            return new Token(keyword, source.Substring(original, keyword.Length), mapping.GetLocation(original));
        }
    }

    public class RegexLexer : ILexer
    {
        public RegexDefinition Regex { get; private set; }
        public RegexLexer(RegexDefinition regex)
        {
            this.Regex = regex;
        }

        public Token TryLex(GrammarDefinition grammar, LineColumnMapping mapping, string source, ref int offset)
        {
            int original = offset;
            if (Regex.Regex.Lex(grammar, source, ref offset))
            {
                return new Token(Regex.Name, source.Substring(original, offset - original), mapping.GetLocation(original));
            }
            else
                return null;
        }
    }
}
