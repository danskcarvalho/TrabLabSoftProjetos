using Compilador.Common;
using Compilador.Lalr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    [Serializable]
    public class GrammarDefinition : GrammarProductionDatabase
    {
        private GrammarDefinition()
        {

        }
        public GrammarDefinition(
            bool isCaseInsensitive, 
            string lineComment, 
            string startBlockComment,
            string endBlockComment, 
            IEnumerable<RegexDefinition> regexDefinitions, 
            IEnumerable<CharsetDefinition> charsetDefinitions, 
            IReadOnlyDictionary<GrammarProduction, string> productionNames,
            IEnumerable<GrammarProduction> productions,
            NonterminalSymbol startingSymbol) : base(productions, startingSymbol)
        {
            IsCaseInsensitive = isCaseInsensitive;
            LineComment = lineComment;
            StartBlockComment = startBlockComment;
            EndBlockComment = endBlockComment;
            ProductionNames = productionNames;
            RegexDefinitions = regexDefinitions.ToList().AsReadOnly();
            CharsetDefinitions = charsetDefinitions.ToList().AsReadOnly();
            ExtractKeywords();
            Validate();
            ComputeTable();

            var charsetByName = new Dictionary<string, CharsetDefinition>();
            this.CharsetByName = charsetByName;
            foreach (var item in CharsetDefinitions)
            {
                charsetByName[item.Name] = item;
            }

            var regexByName = new Dictionary<string, RegexDefinition>();
            this.RegexByName = regexByName;
            foreach (var item in RegexDefinitions)
            {
                regexByName[item.Name] = item;
            }
        }

        public void WriteToFile(string path)
        {
            MemoryStream memorystream = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memorystream, this);
            File.WriteAllBytes(path, memorystream.ToArray());
        }
        public static GrammarDefinition ReadFromFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            MemoryStream memorystream = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            return (Grammar.GrammarDefinition)bf.Deserialize(memorystream);
        }
        public static GrammarDefinition ReadFromStream(Stream stream)
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (Grammar.GrammarDefinition)bf.Deserialize(stream);
        }

        private void ComputeTable()
        {
            Table = Lalr.LalrContext.ComputeTable(this);
            if (Table.HasConflicts)
                throw new GrammarException(new Location(), string.Join("\n", Table.Conflicts));
        }

        private void ExtractKeywords()
        {
            Keywords = new HashSet<string>(this.Productions.SelectMany(x => x.Body.Where(y => y is TerminalSymbol).Cast<TerminalSymbol>()
                        .Where(y => !RegexDefinitions.Any(z => z.Name == y.Name))).Select(x => x.Name)).ToList().AsReadOnly();
        }

        public bool IsCaseInsensitive { get; private set; }
        public string LineComment { get; private set; }
        public string StartBlockComment { get; private set; }
        public string EndBlockComment { get; private set; }
        public IReadOnlyList<RegexDefinition> RegexDefinitions { get; private set; }
        public IReadOnlyList<CharsetDefinition> CharsetDefinitions { get; private set; }
        public IReadOnlyDictionary<GrammarProduction, string> ProductionNames { get; private set; }
        public IReadOnlyList<string> Keywords { get; private set; }
        public LalrTable Table { get; private set; }
        public IReadOnlyDictionary<string, CharsetDefinition> CharsetByName { get; private set; }
        public IReadOnlyDictionary<string, RegexDefinition> RegexByName { get; private set; }

        private void Validate()
        {
            ValidateOptions();
            ValidateNoReservedCharsetName();
            ValidateNoUndeclaredCharsets();
            ValidateNoRecursionCharset();
            ValidateNoUndeclaredNonterminal();
            ValidateNoRecursionTerminal();
            ValidateNoInfRecursion();
        }

        private void ValidateNoRecursionTerminal()
        {
            HashSet<string> stack = new HashSet<string>();
            foreach (var def in RegexDefinitions)
            {
                stack.Clear();
                stack.Add(def.Name);
                ValidateNoRecursionTerminal(def.Regex, stack);
            }
        }

        private void ValidateNoRecursionTerminal(Regex r, HashSet<string> stack)
        {
            if(r is ReferenceRegex)
            {
                var rr = r as ReferenceRegex;
                if (stack.Contains(rr.Reference))
                    throw new GrammarException(r.Location, $"recursão infinita para o símbolo terminal {rr.Reference}");
                if (!RegexDefinitions.Any(x => x.Name == rr.Reference))
                    throw new GrammarException(r.Location, $"terminal não declarado {rr.Reference}");

                stack.Add(rr.Reference);
                ValidateNoRecursionTerminal(RegexDefinitions.First(x => x.Name == rr.Reference).Regex, stack);
            }
            else if(r is CharsetRegex)
            {
                var cr = r as CharsetRegex;
                if(!CharsetDefinitions.Any(x => x.Name == cr.CharsetName) && !IsReservedCharsetName(cr.CharsetName))
                    throw new GrammarException(r.Location, $"charset não declarado {{{cr.CharsetName}}}");
            }
            else
            {
                foreach (var item in r.Children)
                {
                    ValidateNoRecursionTerminal(item, stack);
                }
            }
        }

        private void ValidateNoRecursionCharset()
        {
            HashSet<string> stack = new HashSet<string>();
            foreach (var def in CharsetDefinitions)
            {
                stack.Clear();
                stack.Add(def.Name);
                ValidateNoRecursionCharset(def.Expression, stack);
            }
        }

        private void ValidateNoRecursionCharset(CharsetExpression r, HashSet<string> stack)
        {
            if (r is CharsetNameExpression)
            {
                var rr = r as CharsetNameExpression;
                if (stack.Contains(rr.Name))
                    throw new GrammarException(r.Location, $"recursão infinita para {{{rr.Name}}}");

                stack.Add(rr.Name);
                ValidateNoRecursionCharset(CharsetDefinitions.First(x => x.Name == rr.Name).Expression, stack);
            }
            else if (r is CharsetBinaryExpression)
            {
                var b = r as CharsetBinaryExpression;
                ValidateNoRecursionCharset(b.Left, stack);
                ValidateNoRecursionCharset(b.Right, stack);
            }
        }

        private void ValidateNoInfRecursion()
        {
            HashSet<NonterminalSymbol> finites = new HashSet<NonterminalSymbol>();
            int finitesSize = 0;

            do
            {
                finitesSize = finites.Count;
                foreach (var item in Productions)
                {
                    if (IsFinite(item, finites))
                        finites.Add(item.Head);
                }
            } while (finitesSize != finites.Count);

            var infinites = new HashSet<NonterminalSymbol>();
            foreach (var item in Nonterminals)
            {
                if (!finites.Contains(item))
                    infinites.Add(item);
            }
            if (infinites.Count != 0)
                throw new GrammarException(new Location(), $"não-terminais possuem recursão infinita {string.Join(" ", infinites)}");
        }

        private bool IsFinite(GrammarProduction item, HashSet<NonterminalSymbol> finites)
        {
            if (item.Body.Count == 0)
                return true;
            if (item.Body.All(x => x is TerminalSymbol))
                return true;
            foreach (var e in item.Body)
            {
                if(e is NonterminalSymbol)
                {
                    if (!finites.Contains(e as NonterminalSymbol))
                        return false;
                }
            }
            return true;
        }

        private void ValidateNoUndeclaredNonterminal()
        {
            foreach (var item in Productions)
            {
                foreach (var nt in item.Body.Where(x => x is NonterminalSymbol).Cast<NonterminalSymbol>())
                {
                    if (!Productions.Any(x => x.Head == nt))
                        throw new GrammarException(new Location(), $"símbolo {nt} não declarado");
                }
            }
        }

        private void ValidateNoReservedCharsetName()
        {
            foreach (var item in CharsetDefinitions)
            {
                if (IsReservedCharsetName(item.Name))
                    throw new GrammarException(item.Location, $"nome {{{item.Name}}} é reservado");
            }
        }

        private bool IsReservedCharsetName(string name)
        {
            return name == "Digit" || name == "Letter" || name == "LetterOrDigit" || name == "Number" ||
                name == "Punctuation" || name == "Separator" || name == "Symbol" || name == "Upper" || name == "Whitespace" ||
                name == "Lower" || name == "Any" || name == "None";
        }

        private void ValidateNoUndeclaredCharsets()
        {
            foreach (var item in CharsetDefinitions)
            {
                ValidateNoUndeclaredCharsets(item.Expression);
            }
        }

        private void ValidateNoUndeclaredCharsets(CharsetExpression expr)
        {
            if(expr is CharsetBinaryExpression)
            {
                var b = expr as CharsetBinaryExpression;
                ValidateNoUndeclaredCharsets(b.Left);
                ValidateNoUndeclaredCharsets(b.Right);
            }
            else if(expr is CharsetNameExpression)
            {
                var b = expr as CharsetNameExpression;
                if (!IsReservedCharsetName(b.Name) && !CharsetDefinitions.Any(x => x.Name == b.Name))
                    throw new GrammarException(b.Location, $"nome {{{b.Name}}} não declarado");
            }
        }

        private void ValidateOptions()
        {
            if (LineComment != null)
            {
                if (StartBlockComment == LineComment)
                    throw new GrammarException(new Location(), "símbolo de comentário de linha igual a símbolo de comentário de bloco");
                if (EndBlockComment == LineComment)
                    throw new GrammarException(new Location(), "símbolo de comentário de linha igual a símbolo de comentário de bloco");
            }
            if (StartBlockComment != null)
            {
                if (StartBlockComment == EndBlockComment)
                    throw new GrammarException(new Location(), "símbolos de comentário comentário de bloco iguais");
            }
            var startingSymbol = (this[StartingSymbol][0].Body[0] as NonterminalSymbol);
            if(startingSymbol == null || !this.Productions.Any(x => x.Head == startingSymbol))
                throw new GrammarException(new Location(), "símbolo inicial inexistente");
        }
    }
}
