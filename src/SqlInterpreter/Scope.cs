using System;
using System.Collections.Generic;

namespace SqlInterpreter
{
    public class Scope
    {
        private Dictionary<string, SqlValue> _Values = new Dictionary<string, SqlValue>();
        public IReadOnlyDictionary<string, SqlValue> Values => _Values;

        public Scope()
        {

        }
        public Scope(IReadOnlyDictionary<string, SqlValue> values)
        {
            foreach (var item in values)
            {
                this._Values[item.Key] = item.Value;
            }
        }

        public SqlValue Get(string name){
            if(_Values.ContainsKey(name))
                return _Values[name];
            else
                throw new Exception($"{name} não é um nome conhecido no escopo atual.");
        }

        public static Scope Merge(Scope a, Scope b)
        {
            Dictionary<string, SqlValue> dicA = new Dictionary<string, SqlValue>();
            foreach (var item in a._Values)
            {
                dicA[item.Key] = item.Value;
            }
            foreach (var item in b._Values)
            {
                if (dicA.ContainsKey(item.Key))
                    throw new Exception($"o nome {item.Key} já existe num escopo anterior.");
                dicA[item.Key] = item.Value;
            }
            return new Scope(dicA);
        }
        public static Scope Merge(string name, SqlValue value, Scope oldScope)
        {
            var newScope = new Dictionary<string, SqlValue>{
                {name, value}
            };
            if (oldScope != null)
            {
                foreach (var item in oldScope._Values)
                {
                    if (newScope.ContainsKey(item.Key))
                        throw new Exception($"o nome {name} já existe num escopo anterior.");
                    newScope[item.Key] = item.Value;
                }
            }
            return new Scope(newScope);
        }
    }
}
