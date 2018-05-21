﻿using Compilador.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Grammar
{
    public enum CharsetBinaryOperator
    {
        Minus,
        Plus,
        Div
    }
    public class CharsetBinaryExpression : CharsetExpression
    {
        public CharsetBinaryOperator Operator { get; private set; }
        public CharsetExpression Left { get; private set; }
        public CharsetExpression Right { get; private set; }

        public CharsetBinaryExpression(CharsetBinaryOperator @operator, CharsetExpression left, CharsetExpression right, Location location) : base(location)
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return $"{Left.ToString()} {GetOperatorName()} {Right.ToString()}";
        }

        private string GetOperatorName()
        {
            switch (Operator)
            {
                case CharsetBinaryOperator.Minus:
                    return "-";
                case CharsetBinaryOperator.Plus:
                    return "+";
                case CharsetBinaryOperator.Div:
                    return "/";
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
