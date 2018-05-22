using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class CharsetNameExpression : CharsetExpression
    {
        public string Name { get; set; }

        public CharsetNameExpression(string name, Location location) : base(location)
        {
            Name = name;
        }
        private CharsetNameExpression()
        {

        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Contains(GrammarDefinition grammar, char c)
        {
            switch (Name)
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
                    return grammar.CharsetByName[Name].Expression.Contains(grammar, c);
            }
        }
    }
}
