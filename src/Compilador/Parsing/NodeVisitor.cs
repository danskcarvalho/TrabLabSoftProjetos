using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilador.Parsing
{
    public class NodeVisitor
    {
        public Node Node { get; private set; }

        public NodeVisitor(Node node)
        {
            this.Node = node;
        }

        private Dictionary<NodeType, Action<Node>> _On = new Dictionary<NodeType, Action<Node>>();
        public void On(NodeType nt, Action<Node> fn)
        {
            _On[nt] = fn;
        }
        public void Emit<T>(T obj)
        {
            if(_VisitedStack.Count != 0)
            {
                for (int i = _VisitedStack.Count - 1; i >= 0; i--)
                {
                    var t = obj?.GetType() ?? typeof(T);
                    while (t != null && t != typeof(object))
                    {
                        if (_VisitedStack[i].OnEmit.ContainsKey(t))
                        {
                            _VisitedStack[i].OnEmit[t](obj);
                            goto OutOfFor;
                        }
                        t = t.BaseType;
                    }
                }
                OutOfFor:;
            }
            else
            {
                var t = obj?.GetType() ?? typeof(T);
                while (t != null && t != typeof(object))
                {
                    if (_OnEmit.ContainsKey(t))
                    {
                        _OnEmit[t](obj);
                        break;
                    }
                    t = t.BaseType;
                }
            }
        }

        private Dictionary<Type, Action<object>> _OnEmit = new Dictionary<Type, Action<object>>();
        public void OnEmit<T>(Action<T> fn)
        {
            if (_VisitedStack.Count != 0)
            {
                var lastStackItem = _VisitedStack[_VisitedStack.Count - 1];
                lastStackItem.OnEmit[typeof(T)] = (obj) =>
                {
                    fn((T)obj);
                };
            }
            else
            {
                _OnEmit[typeof(T)] = (obj) =>
                {
                    fn((T)obj);
                };
            }
        }
        
        public void OnFinished(Action fn)
        {
            if (_VisitedStack.Count != 0)
            {
                var lastStackItem = _VisitedStack[_VisitedStack.Count - 1];
                lastStackItem.OnFinished = () =>
                {
                    fn();
                };
            }
            else
                throw new InvalidOperationException();
        }
        
        public void Visit()
        {
            Visit(Node);
        }

        private void Visit(Node node)
        {
            _VisitedStack.Add(new StackItem());
            if (_On.ContainsKey(node.Type))
                _On[node.Type](node);

            foreach (var item in node.Children)
            {
                Visit(item);
            }

            var currentStackItem = _VisitedStack[_VisitedStack.Count - 1];
            _VisitedStack.RemoveAt(_VisitedStack.Count - 1);
            currentStackItem.OnFinished?.Invoke();
        }

        private List<StackItem> _VisitedStack = new List<StackItem>();
        private class StackItem
        {
            public Dictionary<Type, Action<object>> OnEmit { get; set; }
            public Action OnFinished { get; set; }

            public StackItem()
            {
                OnEmit = new Dictionary<Type, Action<object>>();
            }
        }
    }
}
