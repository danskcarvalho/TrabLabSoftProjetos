using API.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Semantics
{
    public class InheritedAttribute<T> : IGrammarAttribute<T>
    {
        private Dictionary<string, Func<Node, T>> Productions;
        public InheritedAttribute(
            Action<SynthesizedAttributeContext<T>> action)
        {
            var ctx = new SynthesizedAttributeContext<T>();
            action(ctx);

            Productions = ctx.Productions;
        }

        public T this[Node node] => Compute(node);

        private Dictionary<Node, AttributeValue<T>> CachedValues = new Dictionary<Node, AttributeValue<T>>();

        private AttributeValue<T> InternalCompute(Node node)
        {
            if (Productions.ContainsKey(node.Name))
                return Productions[node.Name](node);
            else
            {
                var result = AttributeValue<T>.Undefined;
                var current = node;
                while (!result.HasValue && current != null)
                {
                    if (Productions.ContainsKey(current.Name))
                        return Productions[current.Name](node);
                    current = node.Parent;
                }

                return result;
            }
        }

        public T Compute(Node node)
        {
            if (CachedValues.ContainsKey(node))
            {
                if (!CachedValues[node].HasValue)
                    throw new InvalidOperationException("no value computed");
                else
                    return CachedValues[node].Value;
            }

            var c = CachedValues[node] = InternalCompute(node);
            if (!c.HasValue)
                throw new InvalidOperationException("no value computed");
            else
                return c.Value;
        }

        public AttributeValue<T> TryCompute(Node node)
        {
            if (CachedValues.ContainsKey(node))
                return CachedValues[node];
            return CachedValues[node] = InternalCompute(node);
        }
    }
}
