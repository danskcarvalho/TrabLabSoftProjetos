using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SqlInterpreter
{
    public enum SqlValueType {
        Object,
        String,
        Number,
        Boolean,
        List,
        Null
    }
    public class SqlValue : IEquatable<SqlValue>, IComparable<SqlValue>
    {
        public static readonly SqlValue Null = new SqlValue();

        public SqlValueType Type { get; private set; }
        public IReadOnlyDictionary<string, SqlValue> Object { get; private set; }
        public string String { get; private set; }
        public decimal Number { get; private set; }
        public bool Boolean { get; private set; }
        public IReadOnlyList<SqlValue> List { get; private set; }
        public bool IsNull => Type == SqlValueType.Null;
        public bool IsList => Type == SqlValueType.List;
        public bool IsBoolean => Type == SqlValueType.Boolean;
        public bool IsObject => Type == SqlValueType.Object;
        public bool IsString => Type == SqlValueType.String;
        public bool IsNumber => Type == SqlValueType.Number;

        private SqlValue()
        {
            Type = SqlValueType.Null;
        }
        private SqlValue(IEnumerable<SqlValue> list)
        {
            Type = SqlValueType.List;
            List = list.ToList().AsReadOnly();
        }
        private SqlValue(bool boolean)
        {
            Type = SqlValueType.Boolean;
            this.Boolean = boolean;
        }
        private SqlValue(decimal number)
        {
            Type = SqlValueType.Number;
            this.Number = number;
        }
        private SqlValue(IReadOnlyDictionary<string, SqlValue> @object)
        {
            Type = SqlValueType.Object;
            Dictionary<string, SqlValue> dic = new Dictionary<string, SqlValue>();
            foreach (var item in @object)
            {
                dic[item.Key] = @object[item.Key];
            }
            Object = new ReadOnlyDictionary<string, SqlValue>(dic);
        }
        private SqlValue(string @string)
        {
            Type = SqlValueType.String;
            this.String = @string;
        }

        public static implicit operator SqlValue(bool boolean) => new SqlValue(boolean);
        public static implicit operator SqlValue(List<SqlValue> list) => new SqlValue(list);
        public static implicit operator SqlValue(SqlValue[] array) => new SqlValue(array);
        public static implicit operator SqlValue(decimal number) => new SqlValue(number);
        public static implicit operator SqlValue(int number) => new SqlValue(number);
        public static implicit operator SqlValue(Dictionary<string, SqlValue> @object) => new SqlValue(@object);
        public static implicit operator SqlValue(string @string) => new SqlValue(@string);

        public SqlValue Clone() {
            switch (Type)
            {
                case SqlValueType.Boolean:
                    return new SqlValue(Boolean);
                case SqlValueType.List:
                    {
                        return new SqlValue(List.Select(x => x.Clone()).ToList().AsReadOnly());
                    }
                case SqlValueType.Null:
                    return new SqlValue();
                case SqlValueType.Number:
                    return new SqlValue(Number);
                case SqlValueType.Object:
                    {
                        Dictionary<string, SqlValue> dic = new Dictionary<string, SqlValue>();
                        foreach (var key in Object.Keys)
                        {
                            dic[key] = Object[key].Clone();
                        }
                        return new SqlValue(dic);
                    }
                case SqlValueType.String:
                    return new SqlValue(String);
                default:
                    throw new InvalidOperationException();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqlValue);
        }

        public override int GetHashCode()
        {
            switch (Type)
            {
                case SqlValueType.Boolean:
                    return this.Boolean ? 1 : 2;
                case SqlValueType.List:
                    {
                        int hash = 0;
                        for (int i = 0; i < this.List.Count; i++)
                        {
                            hash = 31 * hash + this.List[i].GetHashCode();
                        }
                        return hash;
                    }
                case SqlValueType.Null:
                    return 0;
                case SqlValueType.Number:
                    return this.Number.GetHashCode();
                case SqlValueType.Object:
                    {
                        int hash = 0;
                        foreach (var key in this.Object.Keys.OrderBy(x => x))
                        {
                            hash = 31 * hash + key.GetHashCode();
                            hash = 31 * hash + this.Object[key].GetHashCode();
                        }
                        return hash;
                    }
                case SqlValueType.String:
                    return this.String.GetHashCode();
                default:
                    throw new InvalidOperationException();
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case SqlValueType.Boolean:
                    return this.Boolean ? "true" : "false";
                case SqlValueType.List:
                    {
                        return "[" + string.Join(", ", List.Select(x => x.ToString())) + "]";
                    }
                case SqlValueType.Null:
                    return "null";
                case SqlValueType.Number:
                    return Number.ToString();
                case SqlValueType.Object:
                    {
                        return "{" + string.Join(", ", Object.Select(x => x.Key + ": " + x.Value.ToString())) + "}";
                    }
                case SqlValueType.String:
                    return this.String;
                default:
                    throw new InvalidOperationException();
            }
        }

        public bool Equals(SqlValue other)
        {
            if (other == null)
                return false;

            if (this.Type != other.Type)
                return false;

            switch (Type)
            {
                case SqlValueType.Boolean:
                    return this.Boolean == other.Boolean;
                case SqlValueType.List:
                    {
                        if (this.List.Count != other.List.Count)
                            return false;

                        for (int i = 0; i < this.List.Count; i++)
                        {
                            if (!object.Equals(this.List[i], other.List[i]))
                                return false;
                        }

                        return true;
                    }
                case SqlValueType.Null:
                    return true;
                case SqlValueType.Number:
                    return this.Number == other.Number;
                case SqlValueType.Object:
                    {
                        if (this.Object.Count != other.Object.Count)
                            return false;
                        foreach (var key in Object.Keys)
                        {
                            if (!other.Object.ContainsKey(key) || !object.Equals(other.Object[key], this.Object[key]))
                                return false;
                        }
                        return true;
                    }
                case SqlValueType.String:
                    return this.String == other.String;
                default:
                    throw new InvalidOperationException();
            }
        }

        public int CompareTo(SqlValue other)
        {
            if (this.Type != other.Type)
                return this.Type.ToString().CompareTo(other.Type);

            switch (Type)
            {
                case SqlValueType.Boolean:
                    return this.Boolean.CompareTo(other.Boolean);
                case SqlValueType.List:
                    {
                        if (this.List.Count != other.List.Count)
                            return this.List.Count - other.List.Count;

                        for (int i = 0; i < this.List.Count; i++)
                        {
                            var cp = this.List[i].CompareTo(other.List[i]);
                            if (cp != 0)
                                return cp;
                        }

                        return 0;
                    }
                case SqlValueType.Null:
                    return 0;
                case SqlValueType.Number:
                    return this.Number.CompareTo(other.Number);
                case SqlValueType.Object:
                    {
                        if (this.Object.Count != other.Object.Count)
                            return this.Object.Count - other.Object.Count;
                        var pairs = this.Object.OrderBy(x => x.Key).ThenBy(x => x.Value);
                        var otherPairs = other.Object.OrderBy(x => x.Key).ThenBy(x => x.Value);
                        foreach (var item in pairs.Zip(otherPairs, (arg1, arg2) => Tuple.Create(arg1, arg2)))
                        {
                            var cp = string.Compare(item.Item1.Key, item.Item2.Key, StringComparison.Ordinal);
                            if (cp != 0)
                                return cp;
                            cp = item.Item1.Value.CompareTo(item.Item2.Value);
                            if (cp != 0)
                                return cp;
                        }
                        return 0;
                    }
                case SqlValueType.String:
                    return string.Compare(this.String, other.String, StringComparison.Ordinal);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static bool operator ==(SqlValue a, SqlValue b) => object.Equals(a, b);
        public static bool operator !=(SqlValue a, SqlValue b) => !object.Equals(a, b);
        public static bool operator >(SqlValue a, SqlValue b) => a.CompareTo(b) > 0;
        public static bool operator <(SqlValue a, SqlValue b) => a.CompareTo(b) < 0;
        public static bool operator >=(SqlValue a, SqlValue b) => a.CompareTo(b) >= 0;
        public static bool operator <=(SqlValue a, SqlValue b) => a.CompareTo(b) <= 0;
    }
}
