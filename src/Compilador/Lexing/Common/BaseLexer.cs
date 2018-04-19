using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Lexing
{
    public abstract class BaseLexer : ILexer
    {
        private LineColumnMapping _Mapping;
        private string _Source;

        public BaseLexer(LineColumnMapping mapping, string source)
        {
            _Mapping = mapping;
            _Source = source;
        }

        public abstract Token TryLex(ref int offset);

        protected char CharAt(int offset)
        {
            if (offset >= _Source.Length)
                return '\0';
            return _Source[offset];
        }

        protected string Substring(int start, int end)
        {
            return _Source.Substring(start, (end - start) + 1);
        }

        protected Location GetLocation(int offset)
        {
            return _Mapping.GetLocation(offset);
        }
    }
}
