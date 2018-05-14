using System;
using System.Collections.Generic;

namespace Compilador.Common
{
    public static class Memoize
    {
        private static Dictionary<MemoizationInput, object> mMemoized = new Dictionary<MemoizationInput, object>();
        public static TOutput Function<TInput, TOutput>(FunctionName fn, TInput input, Func<TInput, TOutput> body)
        {
            var key = new MemoizationInput() { FName = fn, Input = input };
            if (mMemoized.ContainsKey(key))
                return (TOutput)mMemoized[key];

            TOutput result = body(input);
            mMemoized[key] = result;
            return result;
        }
        public static FunctionName Function(string name) => new FunctionName(name);

        public class FunctionName {
            public string Name { get; private set; }
            public FunctionName(string name){
                this.Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }
        private class MemoizationInput : IEquatable<MemoizationInput> {
            public FunctionName FName { get; set; }
            public object Input { get; set; }

            public bool Equals(MemoizationInput other)
            {
                if (other == null)
                    return false;

                return other.FName == this.FName && (object.ReferenceEquals(this.Input, other.Input) ||
                                                    (!object.ReferenceEquals(this.Input, null) && this.Input.Equals(other.Input)));
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as MemoizationInput);
            }

            public override int GetHashCode()
            {
                return FName.GetHashCode() * 31 + (Input?.GetHashCode() ?? 0);
            }
        }
    }
}
