using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class CharsetRegex : Regex
    {
        public string CharsetName { get; private set; }
        public CharsetRegex(string charsetName, Location location) : base(location)
        {
            this.CharsetName = charsetName;
        }
        private CharsetRegex()
        {

        }

        public override string ToString()
        {
            return "{" + CharsetName + "}";
        }

        public override IEnumerable<Regex> Children => new List<Regex>();

        public override bool Lex(GrammarDefinition grammar, string source, ref int offset)
        {
            if (offset >= source.Length)
                return false;

            if (CharsetContains(grammar, CharsetName, source[offset]))
            {
                offset++;
                return true;
            }
            else
                return false;
        }

        public bool CharsetContains(GrammarDefinition grammar, string name, char c)
        {
            switch (name)
            {
                case "Any":
                    return true;
                case "None":
                    return false;
                case "Digit":
                    return char.IsDigit(c);
                case "Letter":
                    return char.IsLetter(c);
                case "LetterOrDigit":
                    return char.IsLetterOrDigit(c);
                case "Number":
                    return char.IsNumber(c);
                case "Punctuation":
                    return char.IsPunctuation(c);
                case "Separator":
                    return char.IsSeparator(c);
                case "Symbol":
                    return char.IsSymbol(c);
                case "Upper":
                    return grammar.IsCaseInsensitive || char.IsUpper(c);
                case "Whitespace":
                    return char.IsWhiteSpace(c);
                case "Lower":
                    return grammar.IsCaseInsensitive || char.IsLower(c);
                default:
                    return grammar.CharsetByName[name].Expression.Contains(grammar, c);
            }
        }
    }
}
