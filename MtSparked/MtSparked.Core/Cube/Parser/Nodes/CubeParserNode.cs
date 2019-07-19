using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MtSparked.Interop.Models;
using ConstantIntNode = MtSparked.Core.Cube.Parser.Nodes.ConstantNode<int>;
using ConstantStringNode = MtSparked.Core.Cube.Parser.Nodes.ConstantNode<string>;

// TODO #55: Separate Nodes Into Separate Namespace
namespace MtSparked.Core.Cube.Parser.Nodes {
    public interface ICubeParserNode {

        Type Type { get; }

        object Evaluate(CubeState state);

    }

    // TODO #56: Investigate Typing System for MtCubed Language
    public abstract class CubeParserNode<T> : ICubeParserNode {

        public abstract T Evaluate(CubeState state);

        object ICubeParserNode.Evaluate(CubeState state) => this.Evaluate(state);

        public Type Type => typeof(T);

    }

    public class BoardList : CubeParserNode<List<PackCard>> {

        public string BoardName { get; private set; }
        public BoardList(string boardName) {
            this.BoardName = boardName;
        }

        public override List<PackCard> Evaluate(CubeState state) => state.Boards[this.BoardName];

    }

    public class BoardConcat : CubeParserNode<List<PackCard>> {

        public CubeParserNode<List<PackCard>>[] Children { get; private set; }
        public BoardConcat(params CubeParserNode<List<PackCard>>[] children) {
            this.Children = children;
        }

        public override List<PackCard> Evaluate(CubeState state) {
            IEnumerable<PackCard> result = new List<PackCard>();
            foreach (CubeParserNode<List<PackCard>> child in this.Children) {
                _ = result.Concat(child.Evaluate(state));
            }
            return result.ToList();

        }

    }

    public class BoardComprehension : CubeParserNode<List<PackCard>> {

        public ComprehensionNode Inner { get; private set; }
        public BoardComprehension(CubeParserNode<List<PackCard>> inner, CubeParserNode<bool> filter) {
            this.Inner = new ComprehensionNode(inner, filter, true);
        }

        public override List<PackCard> Evaluate(CubeState state) {
            ArrayList innerValue = (ArrayList)this.Inner.Evaluate(state);
            return innerValue.Cast<PackCard>().ToList();
        }

    }



    public class ConstantNode<T> : CubeParserNode<T> {

        public T Value { get; private set; }
        public ConstantNode(T value) {
            this.Value = value;
        }

        public override T Evaluate(CubeState state) => this.Value;

    }

    public class VariableNode : ICubeParserNode {

        public string Identifier { get; private set; }
        public VariableNode(string identifier) {
            this.Identifier = identifier;
            this.Type = null;
        }

        public object Evaluate(CubeState state) {
            if (!state.VariableValues.ContainsKey(this.Identifier)) {
                throw new Exception($"Variable {this.Identifier} used before definition");
            }
            return state.VariableValues[this.Identifier];
        }

        public Type Type { get; set; }

    }

    public class AddNode : ICubeParserNode {

        public ICubeParserNode Value { get; private set; }
        public AddNode(ICubeParserNode value) {
            this.Value = value;
        }

        public object Evaluate(CubeState state) {
            object value = this.Value.Evaluate(state);
            if (value is PackCard card) {
                state.Pack.Add(card);
            } else {
                throw new Exception("Tried to add a non-card value to the pack");
            }
            return null;
        }

        public Type Type => typeof(void);

    }

   
    public class ArrayNode : ICubeParserNode {

        public ICubeParserNode[] Children { get; private set; }
        public ArrayNode(ICubeParserNode[] children) {
            this.Children = children;
            this.Type = null;
        }

        public object Evaluate(CubeState state) {
            // TODO 106: Make Arrays Homogenous Types in MtCubed
            return this.Children.Select(c => c.Evaluate(state)).ToList();
        }

        public Type Type { get; private set; }

    }

    // TODO #107: Split Up Function Nodes
    public class FunctionNode : ICubeParserNode {

        public ICubeParserNode[] Children { get; private set; }
        public ValueFunction Function { get; private set; }
        public FunctionNode(ValueFunction function, ICubeParserNode[] children) {
            this.Function = function;
            this.Children = children;
            this.Type = null;
            // Check types for each function
            switch (function) {
            case ValueFunction.Zip:
            case ValueFunction.Concat:
                if (this.Children.Length <= 1) {
                    throw new Exception("Zip/Concat must have at least two arguments");
                }
                Type firstChild = this.Children[0].Type;
                this.Type = firstChild;
                break;
            case ValueFunction.Rotate:
                if (this.Children.Length != 2) {
                    throw new Exception("Invalid number of arguments for Rotate");
                }
                this.Type = this.Children[0].Type;
                break;
            case ValueFunction.GetBoard:
                if (this.Children.Length != 1) {
                    throw new Exception("Invalid number of arguments for GetBoard");
                }
                this.Type = typeof(List<Deck.BoardItem>);
                break;
            case ValueFunction.GetProperty:
                if (this.Children.Length != 2) {
                    throw new Exception("Invalid number of arguments for GetProperty");
                }
                if (!(this.Children[1] is ConstantStringNode)) {
                    throw new Exception("Second argument to GetProperty must be a constant string");
                }
                // Evaluate string and determine type from there
                break;
            case ValueFunction.Following:
                if (this.Children.Length != 2) {
                    throw new Exception("Invalid number of arguments for Following");
                }
                // Second type should be IEnumerable<S> where S is the first type
                this.Type = this.Children[1].Type;
                break;
            }
        }

        public object Evaluate(CubeState state) {
            object[] values = this.Children.Select(n => n.Evaluate(state)).ToArray();
            switch (this.Function) {
            case ValueFunction.Zip:
                IList[] realValues = values.Select(v => v as IList).ToArray();
                foreach (object tvalue in realValues) {
                    if (tvalue is null) {
                        throw new Exception("Non-Enumerable type passed to Zip");
                    }
                }
                int maxSize = realValues.Select(e => e.Count).Max();
                return Enumerable.Range(0, maxSize)
                                    .Select(i => Enumerable.Range(0, realValues.Length)
                                                        .Select(j => realValues[j][i])
                                                        .ToArray())
                                    .ToList();
            case ValueFunction.Rotate:
                IList realValue = values[0] as IList;
                if (realValue is null) {
                    throw new Exception("Non-Enumerable variable passed as first argument to Rotate");
                }
                int? tempCount = values[1] as int?;
                if (tempCount is null) {
                    throw new Exception("Non-Integer variable passed as second argument to Rotate");
                }
                int count = (int)tempCount;
                return Enumerable.Range(0, realValue.Count).Select(i => new[] { realValue[i], realValue[(i + count) % realValue.Count] }).ToList();
            case ValueFunction.GetBoard:
                string value = values[0] as string;
                if (value is null) {
                    throw new Exception("Non-String variable passed as argument to GetBoard");
                }
                return new BoardList(value);
            case ValueFunction.Concat:
                CubeParserNode<List<PackCard>>[] boards = values.Select(v => v as CubeParserNode<List<PackCard>>).ToArray();
                bool useBoard = true;
                foreach (CubeParserNode<List<PackCard>> board in boards) {
                    useBoard &= !(board is null);
                }
                if (useBoard) {
                    return new BoardConcat(boards);
                }
                IList[] realValues2 = values.Select(v => v as IList).ToArray();
                foreach (object tvalue in realValues2) {
                    if (tvalue is null) {
                        throw new Exception("Non-Enumerable type passed to Concat");
                    }
                }
                IList result = new ArrayList();
                for (int i = 1; i < realValues2.Length; i++) {
                    foreach (object o in realValues2[i]) {
                        _ = result.Add(o);
                    }
                }
                return result;
            case ValueFunction.GetProperty:
                if (values[0] is PackCard card) {
                    return card.GetProperty((string)values[1]);
                } else {
                    return values[0].GetType().GetProperty((string)values[1]).GetValue(values[0]);
                }
            case ValueFunction.Following:
                IList realValue2 = values[1] as IList;
                if (realValue2 is null) {
                    throw new Exception("Non-Enumerable type passed as second argument to Following");
                }
                int index = realValue2.IndexOf(values[0]);
                if (index < 0) {
                    throw new Exception("Value was not found in list in Following");
                }
                return realValue2[(index + 1) % realValue2.Count];
            }
            throw new NotImplementedException();
        }

        public Type Type { get; private set; }

    }

    public class AssignNode : ICubeParserNode {
        public VariableNode Identifier { get; private set; }
        public ICubeParserNode Value { get; private set; }
        public AssignNode(VariableNode identifier, ICubeParserNode value) {
            this.Identifier = identifier;
            this.Value = value;
        }

        public object Evaluate(CubeState state) {
            object value = this.Value.Evaluate(state);
            state.VariableValues[this.Identifier.Identifier] = value;
            return value;
        }

        public Type Type => this.Identifier.Type;

    }

    public class ExtractNode : ICubeParserNode {

        // TODO #103: Handle Random Creation Better
        private static readonly Random rnd = new Random();
        public ICubeParserNode Value { get; private set; }
        public VariableNode Identifier { get; private set; }
        public bool Replacement { get; private set; }
        public ExtractNode(ICubeParserNode value, VariableNode identifier, bool replacement) {
            // Check type of value
            this.Value = value;
            this.Identifier = identifier;
            this.Replacement = replacement;
            // Internal type of Value
            this.Type = null;
        }

        public object Evaluate(CubeState state) {
            object inner = this.Value.Evaluate(state);
            if (inner is CubeParserNode<List<PackCard>> board) {
                List<PackCard> cards = board.Evaluate(state);
                if (cards.Count == 0) {
                    throw new Exception("Tried to extract from an empty board");
                }
                int ind = rnd.Next(cards.Count);
                PackCard card = cards[ind];
                if (!this.Replacement) {
                    _ = state.Boards[card.BoardName].Remove(card);
                }
                state.VariableValues[this.Identifier.Identifier] = card;
                return card;
            }
            IList value = inner as IList;
            if (value is null) {
                throw new Exception("Tried to extract from a Non-Enumerable value");
            }
            if (value.Count == 0) {
                throw new Exception("Tried to extract from an empty List");
            }
            int index = rnd.Next(value.Count);
            object val = value[index];
            if (!this.Replacement) {
                value.RemoveAt(index);
            }
            state.VariableValues[this.Identifier.Identifier] = val;
            return val;
        }

        public Type Type { get; private set; }

    }

    public class UnpackNode : ICubeParserNode {

        public ICubeParserNode Value { get; private set; }
        public VariableNode[] Children { get; private set; }
        public UnpackNode(ICubeParserNode value, VariableNode[] children) {
            // Value must be an array type with known size
            this.Value = value;
            this.Children = children;
        }

        public object Evaluate(CubeState state) {
            IList value = this.Value.Evaluate(state) as IList;
            if (value is null) {
                throw new Exception("Tried to unpack from a Non-Enumerable value");
            }
            if (value.Count != this.Children.Length) {
                throw new Exception($"Tried to unpack into {this.Children.Length} variables from Enumerable with {value.Count} elements");
            }
            for (int i = 0; i < value.Count; i++) {
                state.VariableValues[this.Children[i].Identifier] = value[i];
            }
            return value;
        }

        public Type Type => typeof(void);

    }

    public class PropOperatorNode : CubeParserNode<bool> {

        public CubeParserNode<bool>[] Children { get; private set; }
        public PropositionOperator Function { get; private set; }
        public PropOperatorNode(PropositionOperator function, CubeParserNode<bool>[] children) {
            this.Function = function;
            this.Children = children;
            // Check types for each function
            switch (function) {
            case PropositionOperator.And:
            case PropositionOperator.Or:
                if (this.Children.Length != 2) {
                    throw new Exception("And/Or must have two arguments");
                }
                break;
            case PropositionOperator.Not:
                if (this.Children.Length != 1) {
                    throw new Exception("Not must only have one argument");
                }
                break;

            }
        }

        public override bool Evaluate(CubeState state) {
            bool res1 = this.Children[0].Evaluate(state);
            bool res2;
            switch (this.Function) {
            case PropositionOperator.And:
                res2 = this.Children[1].Evaluate(state);
                return res1 && res2;
            case PropositionOperator.Or:
                res2 = this.Children[1].Evaluate(state);
                return res1 || res2;
            case PropositionOperator.Not:
                return !res1;
            default:
                throw new NotImplementedException();
            }
        }

    }

    public class PropFunctionNode : CubeParserNode<bool> {
        public ICubeParserNode[] Children { get; private set; }
        public PropositionFunction Function { get; private set; }
        public PropFunctionNode(PropositionFunction function, ICubeParserNode[] children) {
            this.Function = function;
            this.Children = children;
            // Check types for each function
            switch (function) {
            case PropositionFunction.ContainsAtLeast:
            case PropositionFunction.ContainsExact:
            case PropositionFunction.Contains:
                if (children.Length != 2) {
                    throw new Exception("ContainsAtLeast/Exact must have exactly 2 arguments");
                }
                break;
            case PropositionFunction.Intersects:
            case PropositionFunction.Subset:
                if (children.Length != 2) {
                    throw new Exception("Intersects/Subset must have exactly 2 arguments");
                }
                break;
            case PropositionFunction.Equals:
                if (children.Length != 2) {
                    throw new Exception("Equals must have exactly 2 arguments");
                }
                break;
            default:
                throw new NotImplementedException();
            }
        }

        public override bool Evaluate(CubeState state) {
            object[] values = this.Children.Select(n => n.Evaluate(state)).ToArray();
            switch (this.Function) {
            case PropositionFunction.ContainsAtLeast:
                int? tCount = values[1] as int?;
                if (tCount is null) {
                    throw new ArgumentException("Tried to pass Non-Numeric as second argument to ContainsAtLeast");
                }
                int count = (int)tCount;
                if (values[0] is CubeParserNode<List<PackCard>> board) {
                    return board.Evaluate(state).Count >= count;
                }
                IList value = values[0] as IList;
                if (value is null) {
                    throw new Exception("Tried to pass Non-Enumerable as first argument to ContainsAtLeast");
                }
                return value.Count >= count;
            case PropositionFunction.ContainsExact:
                int? tCount2 = values[1] as int?;
                if (tCount2 is null) {
                    throw new Exception("Tried to pass Non-Numeric as second argument to ContainsExact");
                }
                int count2 = (int)tCount2;
                if (values[0] is CubeParserNode<List<PackCard>> board2) {
                    return board2.Evaluate(state).Count == count2;
                }
                IList value2 = values[0] as IList;
                if (value2 is null) {
                    throw new Exception("Tried to pass Non-Enumerable as first argument to ContainsExact");
                }
                return value2.Count == count2;
            case PropositionFunction.Contains:
                if (values[0] is CubeParserNode<List<PackCard>> board3) {
                    return board3.Evaluate(state).Contains(values[1]);
                }
                IList value3 = values[0] as IList;
                if (value3 is null) {
                    throw new Exception("Tried to pass Non-Enumerable as first argument to Contains");
                }
                return value3.Contains(values[1]);
            case PropositionFunction.Intersects:
                if (values[0] is CubeParserNode<List<PackCard>> lBoard && values[1] is CubeParserNode<List<PackCard>> rBoard) {
                    return lBoard.Evaluate(state).Intersect(rBoard.Evaluate(state)).Count() > 0;
                }
                IEnumerable left = values[0] as IEnumerable;
                if (left is null) {
                    throw new ArgumentException("Tried to pass Non-Enumerable as first argument to Intersects");
                }
                IList right = values[1] as IList;
                if (right is null) {
                    throw new Exception("Tried to pass Non-Enumerable as second argument to Intersects");
                }
                return left.Cast<object>().Intersect(right.Cast<object>()).Count() > 0;
            case PropositionFunction.Subset:
                if (values[0] is CubeParserNode<List<PackCard>> lBoard2 && values[1] is CubeParserNode<List<PackCard>> rBoard2) {
                    List<PackCard> lCards = lBoard2.Evaluate(state);
                    return lCards.Intersect(rBoard2.Evaluate(state)).Count() > lCards.Count;
                }
                IList left1 = values[0] as IList;
                if (left1 is null) {
                    throw new Exception("Tried to pass Non-Enumerable as first argument to Subset");
                }
                IList right1 = values[1] as IList;
                if (right1 is null) {
                    throw new Exception("Tried to pass Non-Enumerable as second argument to Subset");
                }
                return left1.Cast<object>().Intersect(right1.Cast<object>()).Count() == left1.Count;
            case PropositionFunction.Equals:
                return values[0] == values[1];
            default:
                throw new NotImplementedException();
            }
        }

    }

    public class ComprehensionNode : ICubeParserNode {

        public ICubeParserNode Value { get; private set; }
        public CubeParserNode<bool> Criteria { get; private set; }
        public bool IsRecursive { get; private set; }
        public ComprehensionNode(ICubeParserNode value, CubeParserNode<bool> criteria, bool isRecursive = false) {
            this.Value = value;
            this.Criteria = criteria;
            this.Type = null;
            this.IsRecursive = isRecursive;
        }

        public object Evaluate(CubeState state) {
            object pVal = this.Value.Evaluate(state);
            if (pVal is CubeParserNode<List<PackCard>> board && !this.IsRecursive) {
                return new BoardComprehension(board, this.Criteria);
            }
            IList value = pVal as IList;

            if (value is null) {
                throw new Exception("Tried to do a comprehension over a Non-Enumerable value");
            }
            return new ArrayList(value.Cast<object>().Where(x => {
                CubeState subState = new CubeState(state);
                subState.VariableValues["X"] = x;
                if (x is IList inner) {
                    for (int i = 0; i < inner.Count; i++) {
                        subState.VariableValues["X" + i] = inner[i];
                    }
                }
                return this.Criteria.Evaluate(subState);
            }).ToList());
        }

        public Type Type { get; private set; }

    }

    public class RepeatNode : ICubeParserNode {

        public ConstantIntNode Number { get; private set; }
        public ArrayNode Commands { get; set; }
        public RepeatNode(ConstantIntNode number, ArrayNode commands) {
            this.Number = number;
            this.Commands = commands;
        }

        public object Evaluate(CubeState state) {
            int n = this.Number.Evaluate(state);
            for (int i = 0; i < n; i++) {
                _ = this.Commands.Evaluate(state);
            }
            return null;
        }

        public Type Type => typeof(void);

    }
}
