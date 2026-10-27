using System;
using MementoMori.Core;

namespace MementoMori.Regions
{
    public enum RegionOperation { Sequence, IndependentDials, ObserveThenChoose }

    /// <summary>Deterministic local puzzle state; errors never erase other regions.</summary>
    public sealed class RegionPuzzleState
    {
        private readonly GameState game;
        private readonly string id;
        private readonly RegionOperation operation;
        private readonly int[] target;
        private readonly int optionCount;
        private string Key(string suffix) => id + "." + suffix;
        public int Progress => game.GetPuzzleProgress(Key("step"));
        public int Errors => game.GetCounter(Key("errors"));
        public bool Solved => game.Puzzles.GetSolved(id);

        public RegionPuzzleState(GameState state,string puzzleId,RegionOperation mode,int[] solution,int choices)
        {
            game=state ?? throw new ArgumentNullException(nameof(state));
            id=puzzleId;
            operation=mode;
            target=(int[])solution.Clone();
            optionCount=choices;
            if(target.Length==0 || choices<1)throw new ArgumentException("Puzzle requires choices and a solution.");
            foreach(var value in target)if(value<0 || value>=choices)throw new ArgumentException("Invalid solution token.");
        }

        public int Dial(int index) => game.GetPuzzleProgress(Key("dial."+index));
        public int Rotate(int index)
        {
            if(index<0 || index>=target.Length)throw new ArgumentOutOfRangeException(nameof(index));
            if(Solved)return Dial(index);
            var value=(Dial(index)+1)%optionCount;
            game.SetPuzzleProgress(Key("dial."+index),value);
            return value;
        }
        public bool Observe(int index)
        {
            if(index<0 || index>=target.Length)throw new ArgumentOutOfRangeException(nameof(index));
            var key=Key("seen."+index);
            if(game.GetPuzzleProgress(key)>0)return false;
            game.SetPuzzleProgress(key,1);
            return true;
        }
        public bool WasObserved(int index) => game.GetPuzzleProgress(Key("seen."+index))>0;
        public bool Confirm()
        {
            if(Solved)return false;
            if(operation!=RegionOperation.IndependentDials)return false;
            for(var i=0;i<target.Length;i++)if(Dial(i)!=target[i])return Fail(false);
            return game.Puzzles.MarkSolved(id);
        }
        public bool Choose(int token)
        {
            if(Solved || token<0 || token>=optionCount)return false;
            if(operation==RegionOperation.ObserveThenChoose)
            {
                for(var i=0;i<target.Length;i++)if(!WasObserved(i))return Fail(false);
                if(token!=target[target.Length-1])return Fail(false);
                return game.Puzzles.MarkSolved(id);
            }
            if(operation!=RegionOperation.Sequence)return false;
            if(token!=target[Progress])return Fail(true);
            game.SetPuzzleProgress(Key("step"),Progress+1);
            if(Progress==target.Length)game.Puzzles.MarkSolved(id);
            return true;
        }
        private bool Fail(bool resetSequence)
        {
            game.IncrementCounter(Key("errors"));
            if(resetSequence)game.SetPuzzleProgress(Key("step"),0);
            return false;
        }
    }
}
