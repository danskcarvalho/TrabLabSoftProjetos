using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Semantics
{
    public struct AttributeValue<T> : IEquatable<AttributeValue<T>>, IEquatable<T>
    {
        public static AttributeValue<T> Undefined = new AttributeValue<T>();

        private T _Value;
        public T Value
        {
            get
            {
                if (!Defined)
                    throw new InvalidOperationException("not defined");
                return _Value;
            }
        }
        public bool HasValue => Defined;
        private bool Defined;

        public AttributeValue(T value)
        {
            this._Value = value;
            this.Defined = true;
        }

        public override bool Equals(object obj)
        {
            if (obj is T)
                return Equals((T)obj);
            else
                return Equals((AttributeValue<T>)obj);
        }

        public bool Equals(T other)
        {
            return this.HasValue && object.Equals(Value, other);
        }

        public bool Equals(AttributeValue<T> other)
        {
            if (!this.HasValue && !other.HasValue)
                return true;

            if (this.HasValue && other.HasValue)
                return object.Equals(this.Value, other.Value);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return !HasValue ? 0 : (((object)Value)?.GetHashCode() ?? 0);
        }

        public override string ToString()
        {
            return !HasValue ? "undefined" : (((object)Value)?.ToString() ?? "undefined");
        }

        public static implicit operator AttributeValue<T>(T value)
        {
            return new AttributeValue<T>(value);
        }
    }
}
