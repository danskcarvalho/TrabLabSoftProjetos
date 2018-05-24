using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Compilador.Lalr
{
    class LalrContext
    {
        public static LalrTable ComputeTable(GrammarProductionDatabase db){
            var ctx = new LalrContext(db);
            ctx.ComputeStates();
            return ctx.mTable;
        }
        
        private LalrContext(GrammarProductionDatabase db)
        {
            this.mDatabase = db;
        }

        private GrammarProductionDatabase mDatabase;
        private Dictionary<LalrItemSet, int> mItemsByIndex;
        private List<LalrItemSet> mStates;
        private HashSet<LalrItemSet> mCurrentItems;
        private HashSet<LalrItemSet> mNextItems;
        private Dictionary<Tuple<int, Symbol>, int> mRecordedGotos;
        private LalrTable mTable;

        private void ComputeStates(){
            mItemsByIndex = new Dictionary<LalrItemSet, int>();
            mStates = new List<LalrItemSet>();
            mCurrentItems = new HashSet<LalrItemSet>();
            mNextItems = new HashSet<LalrItemSet>();
            mRecordedGotos = new Dictionary<Tuple<int, Symbol>, int>();

            var initialState = new LalrItemSet(new LalrItem[] {
                new LalrItem(mDatabase[NonterminalSymbol.StartingSymbol].First(), 0, TerminalSymbol.Eof)
            });
            //mStates.Add(initialState);
            mStates.Add(initialState.Closure(mDatabase));
            //mItemsByIndex.Add(mStates[0].ToCoreSet(), 0);
            mItemsByIndex.Add(mStates[0], 0);
            mCurrentItems.Add(mStates[0]);

            while (mCurrentItems.Count != 0)
            {
                foreach (var iset in mCurrentItems)
                {
                    Expand(iset);
                }

                mCurrentItems.Clear();
                mCurrentItems.UnionWith(mNextItems);
                mNextItems.Clear();
            }

            CreateTable();
        }

        private void CreateTable()
        {
            List<LalrState> stateList = new List<LalrState>();
            mTable = new LalrTable(stateList);

            for (int i = 0; i < mStates.Count; i++)
            {
                var actions = new Dictionary<Symbol, ReadOnlyCollection<LalrAction>>();
                var tempActions = new Dictionary<Symbol, List<LalrAction>>();
                var st = new LalrState(actions);
                stateList.Add(st);

                var currentItem = mStates[i].Closure(mDatabase);
                foreach (var item in currentItem)
                {
                    if (!item.AtEnd && item.CurrentSymbol is TerminalSymbol)
                    {
                        if (!tempActions.ContainsKey(item.CurrentSymbol))
                            tempActions[item.CurrentSymbol] = new List<LalrAction>();

                        if (tempActions[item.CurrentSymbol].Any(a => a is LalrShift))
                            continue;

                        tempActions[item.CurrentSymbol].Add(new LalrShift(mRecordedGotos[Tuple.Create(i, item.CurrentSymbol)], mTable));
                    }
                    else if(item.AtEnd && item.Production.Head == NonterminalSymbol.StartingSymbol){
                        if (!tempActions.ContainsKey(TerminalSymbol.Eof))
                            tempActions[TerminalSymbol.Eof] = new List<LalrAction>();

                        tempActions[TerminalSymbol.Eof].Add(new LalrAccept());
                    }
                    else if(item.AtEnd){
                        if (!tempActions.ContainsKey(item.Lookahead))
                            tempActions[item.Lookahead] = new List<LalrAction>();

                        if (tempActions[item.Lookahead].Any(a => a is LalrReduce && ((LalrReduce)a).Production == item.Production))
                            continue;

                        tempActions[item.Lookahead].Add(new LalrReduce(item.Production));
                    }
                    else {
                        if (tempActions.ContainsKey(item.CurrentSymbol))
                            continue;

                        tempActions[item.CurrentSymbol] = new List<LalrAction>();
                        tempActions[item.CurrentSymbol].Add(new LalrGoto(mRecordedGotos[Tuple.Create(i, item.CurrentSymbol)], mTable));
                    }
                }

                foreach (var act in tempActions)
                {
                    actions.Add(act.Key, act.Value.AsReadOnly());
                }
            }
        }

        private void Expand(LalrItemSet iset)
        {
            //var thisIndex = mItemsByIndex[iset.ToCoreSet()];
            var thisIndex = mItemsByIndex[iset];

            var performGoto = iset.Goto(mDatabase);
            foreach (var item in performGoto)
            {
                //var cs = item.Value.ToCoreSet();
                //if(mItemsByIndex.ContainsKey(cs)){
                if (mItemsByIndex.ContainsKey(item.Value)) {
                    //var index = mItemsByIndex[cs];
                    var index = mItemsByIndex[item.Value];
                    //mStates[index] = LalrItemSet.Merge(mStates[index], item.Value);
                    mRecordedGotos.Add(Tuple.Create(thisIndex, item.Key), index);
                }
                else {
                    mStates.Add(item.Value);
                    //mItemsByIndex.Add(cs, mStates.Count - 1);
                    mItemsByIndex.Add(item.Value, mStates.Count - 1);
                    mNextItems.Add(item.Value);
                    mRecordedGotos.Add(Tuple.Create(thisIndex, item.Key), mStates.Count - 1); 
                }
            }
        }
    }
}
