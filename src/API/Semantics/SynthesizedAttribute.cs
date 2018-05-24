using API.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Semantics
{
    public class SynthesizedAttributeContext<T>
    {
        internal Dictionary<string, Func<Node, T>> Productions = new Dictionary<string, Func<Node, T>>();
        public void On(string nodeName, Func<Node, T> produceFunc)
        {
            Productions[nodeName] = produceFunc;
        }
    }
    public class SynthesizedAttribute<T> : IGrammarAttribute<T>
    {
        public Func<T, T, T> AggregationFunction { get; private set; }

        private Dictionary<string, Func<Node, T>> Productions;
        public SynthesizedAttribute(
            Action<SynthesizedAttributeContext<T>> action, 
            Func<T, T, T> aggregationFunction)
        {
            var ctx = new SynthesizedAttributeContext<T>();
            action(ctx);

            Productions = ctx.Productions;
            AggregationFunction = aggregationFunction ?? ((a, b) => a);
        }

        public T this[Node node] => Compute(node);

        private Dictionary<Node, AttributeValue<T>> CachedValues = new Dictionary<Node, AttributeValue<T>>();

        private AttributeValue<T> InternalCompute(Node node)
        {
            if (Productions.ContainsKey(node.Name))
                return Productions[node.Name](node);
            else
            {
                if (node is TokenNode)
                    return AttributeValue<T>.Undefined;
                else
                {
                    var rNode = (ReducedNode)node;
                    AttributeValue<T> aggregatedValue = AttributeValue<T>.Undefined;
                    foreach (var child in rNode.Children)
                    {
                        var computed = InternalCompute(child);
                        if (computed.HasValue)
                        {
                            if (!aggregatedValue.HasValue)
                                aggregatedValue = computed;
                            else
                                aggregatedValue = AggregationFunction(aggregatedValue.Value, computed.Value);
                        }
                    }
                    return aggregatedValue;
                }
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
