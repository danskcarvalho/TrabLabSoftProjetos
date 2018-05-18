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
                    var t = typeof(T);
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
                var t = typeof(T);
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
                _VisitedStack[_VisitedStack.Count - 1] = lastStackItem;
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
                _VisitedStack[_VisitedStack.Count - 1] = lastStackItem;
            }
            else
                throw new InvalidOperationException();
        }

        private List<StackItem> _VisitedStack = new List<StackItem>();
        public void Visit()
        {
            List<Node> toVisit = new List<Node>() { Node };
            
            while(toVisit.Count != 0)
            {
                var lastNode = toVisit[toVisit.Count - 1];
                
                //remove and add children
                toVisit.RemoveAt(toVisit.Count - 1);
                if(_VisitedStack.Count != 0)
                {
                    var lastStackItem = _VisitedStack[_VisitedStack.Count - 1];
                    lastStackItem.ChildrenVisited--;
                    if (lastStackItem.ChildrenVisited <= 0)
                    {
                        lastStackItem.OnFinished?.Invoke();
                        _VisitedStack.RemoveAt(_VisitedStack.Count - 1);
                    }
                    else
                        _VisitedStack[_VisitedStack.Count - 1] = lastStackItem;
                }
                foreach(var item in lastNode.Children.Reverse())
                {
                    toVisit.Add(item);
                }

                if (lastNode.Children.Count != 0)
                    _VisitedStack.Add(new StackItem(lastNode, lastNode.Children.Count));
                Visit(lastNode);
            }
        }

        private void Visit(Node node)
        {
            if (_On.ContainsKey(node.Type))
                _On[node.Type](node);
        }

        private struct StackItem
        {
            public Node Node { get; set; }
            public int ChildrenVisited { get; set; }
            public Dictionary<Type, Action<object>> OnEmit { get; set; }
            public Action OnFinished { get; set; }

            public StackItem(Node node, int childrenVisited) : this()
            {
                Node = node;
                ChildrenVisited = childrenVisited;
                OnEmit = new Dictionary<Type, Action<object>>();
            }
        }
    }
}
