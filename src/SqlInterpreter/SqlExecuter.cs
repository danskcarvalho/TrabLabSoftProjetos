using System;
using API.Semantics;
using System.Collections.Generic;
using Compilador.Grammar;
using System.Linq;

namespace SqlInterpreter
{
    public class SqlExecuter : Interpreter
    {
        //GLOBAL COLLECTIONS
        private Dictionary<string, SqlValue> globalCollections = new Dictionary<string, SqlValue>();
        private static GrammarDefinition _SqlGrammar;
        static SqlExecuter(){
            _SqlGrammar = GrammarDefinition.ReadFromResource<SqlExecuter>("SqlInterpreter.Grammar.BNFOutput");
        }
        public SqlExecuter()
        {
        }

        public override GrammarDefinition Grammar => _SqlGrammar;

        protected override void DefineAttributes()
        {
            SynthesizedAttribute<Func<Scope, SqlValue>> selectAttr = null;
            SynthesizedAttribute<Func<Scope, SqlValue>> exprAttr = null;
            SynthesizedAttribute<Func<Scope, SqlValue>> fromSource = Synthesized<Func<Scope, SqlValue>>(
                d =>
                {
                    d.On("FromSourceId", n =>
                    {
                        var id = n.Children[0].Token.Source;
                        return s =>
                        {
                            //get from scope or global collections
                            var obj = s != null ? 
                                (s.Values.ContainsKey(id) ? s.Get(id) : globalCollections[id]) : 
                                globalCollections[id];
                            if (!obj.IsList) //must be list
                                throw new Exception($"{id} não é uma coleção");
                            return obj; //retorna lista (possivelmente de uma coleção global)
                        };
                    });
                    d.On("FromSourceSel", n =>
                    {
                        return s =>
                        {
                            return selectAttr[n.Children[1]](s); //run the select and return the list
                        };
                    });
                }
            );
            SynthesizedAttribute<Func<List<Scope>, List<Scope>>> fromSelect = Synthesized<Func<List<Scope>, List<Scope>>>(
                d =>
                {
                    d.On("FromSelect", n =>
                    {
                        return (listScope) =>
                        {
                            List<Scope> result = new List<Scope>();
                            var source = fromSource[n.Children[3]]; //get the source of this from

                            if (listScope != null) //will be null if the select is top level
                            {
                                foreach (var item in listScope)
                                {
                                    var list = source(item); //este from pode depender de um escopo anterior
                                    if (!list.IsList) //precisa ser lista
                                        throw new Exception($"não é uma lista");
                                    foreach (var sourceItem in list.List) //quadrático
                                    {
                                        //une os dois escopos
                                        result.Add(Scope.Merge(n.Children[1].Token.Source, sourceItem, item));
                                    }
                                }
                            }
                            else
                            {
                                var list = source(null); //não depende de um escopo anterior
                                if (!list.IsList) //precisa ser lista
                                    throw new Exception($"não é uma lista");
                                foreach (var sourceItem in list.List) //cria um escopo para cada objeto
                                {
                                    //1 objeto por escopo
                                    result.Add(Scope.Merge(n.Children[1].Token.Source, sourceItem, null));
                                }
                            }
                            return result;
                        };
                    });
                },
                (a, b) => (c => b(a(c))) // a --> b
            );
            //Filtra baseada numa expressão
            SynthesizedAttribute<Func<Scope, bool>> whereFilter = Synthesized<Func<Scope, bool>>(d => 
            {
                d.On("WhereFilter", n =>
                {
                    return s =>
                    {
                        var r = exprAttr[n.Children[1]](s);
                        if (!r.IsBoolean)
                            throw new Exception("a expressão dentro de um where precisa ser um booleano");
                        return r.Boolean;
                    };
                });
            });
            //Executa o where existente num select
            SynthesizedAttribute<Func<List<Scope>, List<Scope>>> where1 = Synthesized<Func<List<Scope>, List<Scope>>>(d =>
            {
                d.On("WhereGroupByHaving", n =>
                {
                    return list =>
                    {
                        var filter = whereFilter[n.Children[0]];
                        List<Scope> filtered = new List<Scope>();
                        foreach (var item in list)
                        {
                            var b = filter(item);
                            if (b)
                                filtered.Add(item);
                        }
                        return filtered;
                    };
                });
                d.On("WhereGroupBy", n =>
                {
                    return list =>
                    {
                        var filter = whereFilter[n.Children[0]];
                        List<Scope> filtered = new List<Scope>();
                        foreach (var item in list)
                        {
                            var b = filter(item);
                            if (b)
                                filtered.Add(item);
                        }
                        return filtered;
                    };
                });
                d.On("Where", n =>
                {
                    return list =>
                    {
                        var filter = whereFilter[n.Children[0]];
                        List<Scope> filtered = new List<Scope>();
                        foreach (var item in list)
                        {
                            var b = filter(item);
                            if (b)
                                filtered.Add(item);
                        }
                        return filtered;
                    };
                });
            });
            //executa  a filtragem de grupos
            SynthesizedAttribute<Func<List<Scope>, List<Scope>>> having = Synthesized<Func<List<Scope>, List<Scope>>>(d =>
            {
                d.On("WhereGroupByHaving", n =>
                {
                    return list =>
                    {
                        var filter = whereFilter[n.Children[2]];
                        List<Scope> filtered = new List<Scope>();
                        foreach (var item in list)
                        {
                            var b = filter(item);
                            if (b)
                                filtered.Add(item);
                        }
                        return filtered;
                    };
                });
                d.On("GroupByHaving", n =>
                {
                    return list =>
                    {
                        var filter = whereFilter[n.Children[1]];
                        List<Scope> filtered = new List<Scope>();
                        foreach (var item in list)
                        {
                            var b = filter(item);
                            if (b)
                                filtered.Add(item);
                        }
                        return filtered;
                    };
                });
            });
            //Agrupamento do select
            SynthesizedAttribute<Func<List<Scope>, List<Scope>>> groupBy = Synthesized<Func<List<Scope>, List<Scope>>>(d =>
            {
                d.On("GroupByClause", n =>
                {
                    //groupby _ into <intoId>
                    var intoId = n.Children[3].Token.Source;
                    return list =>
                    {
                        Dictionary<string, API.Parsing.Node> groupByExpressions = new Dictionary<string, API.Parsing.Node>();
                        GetGroupByExpressionNodes(groupByExpressions, n.Children[1]); //pega todos as keys do groupby

                        //agrupa pelas keys
                        var grouped = list.GroupBy(s => {
                            Dictionary<string, SqlValue> obj = new Dictionary<string, SqlValue>();
                            foreach (var item in groupByExpressions)
                            {
                                obj[item.Key] = exprAttr[item.Value](s);
                            }
                            return (SqlValue)obj;
                        }).ToList();

                        //transforma os agrupamentos em escopos
                        var listScope = grouped.Select(x =>
                        {
                            Dictionary<string, SqlValue> scoped = new Dictionary<string, SqlValue>();
                            foreach (var item in x.Key.Object)
                            {
                                scoped[item.Key] = item.Value;
                            }

                            scoped[intoId] = x.Select(y => ScopeToValue(y)).ToList();
                            return new Scope(scoped);
                        }).ToList();
                        //retorna uma nova lista
                        return listScope;
                    };
                });
            });
            //ordenamento
            SynthesizedAttribute<Func<List<Scope>, List<Scope>>> orderBy = Synthesized<Func<List<Scope>, List<Scope>>>(d =>
            {
                d.On("OrderByClause", n =>
                {
                    return list =>
                    {
                        List<Tuple<API.Parsing.Node, string>> orderBys = new List<Tuple<API.Parsing.Node, string>>();
                        GetOrderBys(orderBys, n.Children[1]);
                        IOrderedEnumerable<Scope> orderedScopes = null;
                        foreach (var ob in orderBys)
                        {
                            if (orderedScopes == null)
                                orderedScopes = ob.Item2 == "asc" ?
                                                  list.OrderBy(x => exprAttr[ob.Item1](x)) :
                                                  list.OrderByDescending(x => exprAttr[ob.Item1](x));
                            else
                                orderedScopes = ob.Item2 == "asc" ?
                                                  orderedScopes.ThenBy(x => exprAttr[ob.Item1](x)) :
                                                  orderedScopes.ThenByDescending(x => exprAttr[ob.Item1](x));
                        }
                        return orderedScopes.ToList();
                    };
                });
            });
            //select class
            SynthesizedAttribute<Func<List<Scope>, List<SqlValue>>> selectClause = Synthesized<Func<List<Scope>, List<SqlValue>>>(d =>
            {
                d.On("SelectClause", n =>
                {
                    return list =>
                    {
                        List<SqlValue> result = new List<SqlValue>();
                        foreach (var item in list)
                        {
                            result.Add(exprAttr[n.Children[1]](item));
                            if (!result[result.Count - 1].IsObject)
                                throw new Exception("valor retornado por select precisa ser um objeto.");
                        }
                        return result;
                    };
                });
            });
            selectAttr = Synthesized<Func<Scope, SqlValue>>(d => {
                d.On("Select", n =>
                {
                    return s =>
                    {
                        var pipeline = fromSelect[n.Children[0]](s != null ? new List<Scope> { s } : null);
                        var wh = where1.TryCompute(n.Children[1]);
                        if (wh.HasValue)
                        {
                            pipeline = wh.Value(pipeline);
                        }
                        var gb = groupBy.TryCompute(n.Children[1]);
                        if(gb.HasValue){
                            pipeline = gb.Value(pipeline);
                        }
                        var hv = having.TryCompute(n.Children[1]);
                        if(hv.HasValue){
                            pipeline = hv.Value(pipeline);
                        }
                        var ob = orderBy.TryCompute(n.Children[2]);
                        if(ob.HasValue){
                            pipeline = ob.Value(pipeline);
                        }

                        return selectClause[n.Children[3]](pipeline);
                    };
                });
            });
            SynthesizedAttribute<Action> insert = Synthesized<Action>(d => {
                d.On("InsertValue", n =>
                {
                    return () =>
                    {
                        var id = n.Children[2].Token.Source;
                        var obj = exprAttr[n.Children[3]](new Scope());
                        if (!globalCollections.ContainsKey(id))
                            globalCollections[id] = new List<SqlValue> { obj };
                        else
                            globalCollections[id] = AddToList(globalCollections[id], obj);
                    };
                });
                d.On("InsertSelect", n =>
                {
                    return () =>
                    {
                        var id = n.Children[2].Token.Source;
                        var list = selectAttr[n.Children[4]](null);
                        if (!globalCollections.ContainsKey(id))
                            globalCollections[id] = list;
                        else
                            globalCollections[id] = MergeLists(globalCollections[id], list);
                    };
                });
            });
            SynthesizedAttribute<Action> delete = Synthesized<Action>(d => {
                d.On("DeleteAll", n =>
                {
                    return () =>
                    {
                        var id = n.Children[2].Token.Source;
                        if (globalCollections.ContainsKey(id))
                            globalCollections[id] = new List<SqlValue>();
                    };
                });
                d.On("DeleteWhere", n =>
                {
                    return () =>
                    {
                        var id = n.Children[2].Token.Source;
                        var expr = exprAttr[n.Children[4]];
                        if (!globalCollections.ContainsKey(id))
                            return;

                        List<SqlValue> toBeDeleted = new List<SqlValue>(globalCollections[id].List);
                        toBeDeleted.RemoveAll(x => {
                            var r = expr(ValueToScope(x));
                            if (!r.IsBoolean)
                                throw new Exception("a expressão dentro de um where precisa ser um booleano");
                            return r.Boolean;
                        });
                        globalCollections[id] = toBeDeleted;
                    };
                });
            });
            SynthesizedAttribute<Action> statement = Synthesized<Action>(d => {
                d.On("IsSelect", n =>
                {
                    return () =>
                    {
                        try
                        {
                            var result = selectAttr[n](null);
                            foreach (var item in result.List)
                            {
                                Console.WriteLine(item.ToString());
                            }
                        }
                        catch (Exception e)
                        {
                            DisplayError(e.Message);
                        }
                    };
                });
                d.On("IsInsert", n =>
                {
                    return () =>
                    {
                        try
                        {
                            insert[n]();
                        }
                        catch (Exception e)
                        {
                            DisplayError(e.Message);
                        }
                    };
                });
                d.On("IsDelete", n =>
                {
                    return () =>
                    {
                        try
                        {
                            delete[n]();
                        }
                        catch (Exception e)
                        {
                            DisplayError(e.Message);
                        }
                    };
                });
            });
            exprAttr = Synthesized<Func<Scope, SqlValue>>(d =>
            {
                d.On("Number", n => s => decimal.Parse(n.Children[0].Token.Source));
                d.On("String", n => s => n.Children[0].Token.Source.Substring(1, n.Children[0].Token.Source.Length - 2));
                d.On("Null", n => s => SqlValue.Null);
                d.On("True", n => s => true);
                d.On("False", n => s => false);
                d.On("Round", n => s => Math.Round(exprAttr[n.Children[2]](s).Number));
                d.On("Abs", n => s => Math.Abs(exprAttr[n.Children[2]](s).Number));
                d.On("Ceiling", n => s => Math.Ceiling(exprAttr[n.Children[2]](s).Number));
                d.On("Floor", n => s => Math.Floor(exprAttr[n.Children[2]](s).Number));
                d.On("Max2", n => s => Math.Max(exprAttr[n.Children[2]](s).Number, exprAttr[n.Children[4]](s).Number));
                d.On("Min2", n => s => Math.Min(exprAttr[n.Children[2]](s).Number, exprAttr[n.Children[4]](s).Number));
                d.On("Sign", n => s => Math.Sign(exprAttr[n.Children[2]](s).Number));
                d.On("Trim", n => s => exprAttr[n.Children[2]](s).String.Trim());
                d.On("Contains", n => s => exprAttr[n.Children[2]](s).String.Contains(exprAttr[n.Children[4]](s).String));
                d.On("Any", n => s => {
                    var list = selectAttr[n](s);
                    return list.List.Any();
                });
                d.On("Sum", n => s => {
                    var list = selectAttr[n](s);
                    var dec = list.List.Sum(x => (decimal?)x.Object.First().Value.Number);
                    return dec != null ? (SqlValue)dec.Value : SqlValue.Null;
                });
                d.On("Max1", n => s => {
                    var list = selectAttr[n](s);
                    var dec = list.List.Max(x => (decimal?)x.Object.First().Value.Number);
                    return dec != null ? (SqlValue)dec.Value : SqlValue.Null;
                });
                d.On("Min1", n => s => {
                    var list = selectAttr[n](s);
                    var dec = list.List.Min(x => (decimal?)x.Object.First().Value.Number);
                    return dec != null ? (SqlValue)dec.Value : SqlValue.Null;
                });
                d.On("Avg", n => s => {
                    var list = selectAttr[n](s);
                    var dec = list.List.Average(x => (decimal?)x.Object.First().Value.Number);
                    return dec != null ? (SqlValue)dec.Value : SqlValue.Null;
                });
                d.On("Not", n => s => !(exprAttr[n.Children[2]](s).Boolean));
                d.On("ObjectValue", n =>
                {
                    Dictionary<string, API.Parsing.Node> fields = new Dictionary<string, API.Parsing.Node>();
                    GetObjectFields(fields, n.Children[1]);
                    return s =>
                    {
                        Dictionary<string, SqlValue> obj = new Dictionary<string, SqlValue>();
                        foreach (var item in fields)
                        {
                            obj[item.Key] = exprAttr[item.Value](s);
                            if (obj[item.Key].IsList)
                                throw new Exception($"Campo {item.Key} não pode ser lista");
                        }
                        return obj;
                    };
                });
                d.On("MemberAccess", n =>
                {
                    return s =>
                    {
                        var m = exprAttr[n.Children[0]](s);
                        var id = n.Children[2].Token.Source;
                        if (m.IsNull || !m.Object.ContainsKey(id))
                            return SqlValue.Null;
                        return m.Object[id];
                    };
                });
                d.On("Identifier", n =>
                {
                    return s =>
                    {
                        return s.Get(n.Children[0].Token.Source);
                    };
                });
                d.On("And", n => s => (exprAttr[n.Children[0]](s).Boolean) && (exprAttr[n.Children[2]](s).Boolean));
                d.On("Or", n => s => (exprAttr[n.Children[0]](s).Boolean) || (exprAttr[n.Children[2]](s).Boolean));
                d.On("Eq", n => s => (exprAttr[n.Children[0]](s)) == (exprAttr[n.Children[2]](s)));
                d.On("NotEq", n => s => (exprAttr[n.Children[0]](s)) != (exprAttr[n.Children[2]](s)));
                d.On("Gt", n => s => (exprAttr[n.Children[0]](s)) > (exprAttr[n.Children[2]](s)));
                d.On("Lt", n => s => (exprAttr[n.Children[0]](s)) < (exprAttr[n.Children[2]](s)));
                d.On("GtEq", n => s => (exprAttr[n.Children[0]](s)) >= (exprAttr[n.Children[2]](s)));
                d.On("LtEq", n => s => (exprAttr[n.Children[0]](s)) <= (exprAttr[n.Children[2]](s)));
                d.On("Add", n => s =>
                {
                    var op1 = exprAttr[n.Children[0]](s);
                    var op2 = exprAttr[n.Children[2]](s);
                    return op1.IsNumber ? (SqlValue)(op1.Number + op2.Number) : op1.String + op2.String;
                });
                d.On("Sub", n => s => (exprAttr[n.Children[0]](s).Number) - (exprAttr[n.Children[2]](s).Number));
                d.On("Neg", n => s => -(exprAttr[n.Children[0]](s).Number));
                d.On("Mult", n => s => (exprAttr[n.Children[0]](s).Number) * (exprAttr[n.Children[2]](s).Number));
                d.On("Div", n => s => (exprAttr[n.Children[0]](s).Number) / (exprAttr[n.Children[2]](s).Number));
            });

            base.Interpretation = (n) => statement[n]();
        }

        private static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private SqlValue AddToList(SqlValue list, SqlValue obj){
            List<SqlValue> l = new List<SqlValue>(list.List);
            l.Add(obj);
            return l;
        }
        private SqlValue MergeLists(SqlValue a, SqlValue b)
        {
            List<SqlValue> l = new List<SqlValue>(a.List);
            l.AddRange(b.List);
            return l;
        }
        private void GetObjectFields(Dictionary<string, API.Parsing.Node> dic, API.Parsing.Node root)
        {
            var expression = root.Children[0].Children[2];
            var id = root.Children[0].Children[0].Token.Source;
            dic[id] = expression;
            if (root.Children.Count != 1)
                GetObjectFields(dic, root.Children[2]);
        }
        private void GetGroupByExpressionNodes(Dictionary<string, API.Parsing.Node> dic, API.Parsing.Node root){
            var expression = root.Children[0].Children[0];
            var id = root.Children[0].Children[2].Token.Source;
            dic[id] = expression;
            if (root.Children.Count != 1)
                GetGroupByExpressionNodes(dic, root.Children[2]);
        }
        private void GetOrderBys(List<Tuple<API.Parsing.Node, string>> list, API.Parsing.Node root)
        {
            var expression = root.Children[0].Children[0];
            var asc = root.Children[0].Children.Count != 1 ? root.Children[0].Children[1].Token.Source : "asc";
            list.Add(Tuple.Create(expression, asc));
            if (root.Children.Count != 1)
                GetOrderBys(list, root.Children[2]);
        }

        private SqlValue ScopeToValue(Scope a){
            Dictionary<string, SqlValue> values = new Dictionary<string, SqlValue>();
            foreach (var item in a.Values)
            {
                values[item.Key] = item.Value;
            }
            return (SqlValue)values;
        }
        private Scope ValueToScope(SqlValue a)
        {
            Dictionary<string, SqlValue> values = new Dictionary<string, SqlValue>();
            foreach (var item in a.Object)
            {
                values[item.Key] = item.Value;
            }
            return new Scope(values);
        }
    }
}
