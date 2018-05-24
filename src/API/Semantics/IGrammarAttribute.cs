using API.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Semantics
{
    public interface IGrammarAttribute<T>
    {
        T Compute(Node node);
        AttributeValue<T> TryCompute(Node node);
    }
}
