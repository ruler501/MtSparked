using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using System;
using Pidgin;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using MtSparked.Models;
using Pidgin.Expression;
using System.Collections;

namespace MtSparked.Database
{
    using ConstantIntNode = ConstantNode<int>;
    using ConstantStringNode = ConstantNode<string>;
    using ConstantBoolNode = ConstantNode<bool>;

    public class CubeParser
    {
        // Special symbols
        private static readonly Parser<char, char> Colon = Char(':');
        private static readonly Parser<char, char> LBracket = Char('[');
        private static readonly Parser<char, char> RBracket = Char(']');
        private static readonly Parser<char, char> LParen = Char('(');
        private static readonly Parser<char, char> RParen = Char(')');
        private static readonly Parser<char, char> LBrace = Char('{');
        private static readonly Parser<char, char> RBrace = Char('}');
        private static readonly Parser<char, char> SingleQuote = Char('\'');
        private static readonly Parser<char, char> DoubleQuote = Char('"');
        private static readonly Parser<char, char> Comma = Char(',');
        private static readonly Parser<char, char> Assign = Char('=');
        private static readonly Parser<char, char> Newline = Char('\n');
        private static readonly Parser<char, char> Space = Char(' ');
        private static readonly Parser<char, char> Tab = Char('\t');
        private static readonly Parser<char, char> SingleLineWhitespace = Space.Or(Tab);
        private static readonly Parser<char, Unit> SkipSingleLineWhitespace = SingleLineWhitespace.SkipMany().Labelled("Single Line Whitespace");

        // Keyword
        private static readonly Parser<char, string> Where = String("where");
        private static readonly Parser<char, string> Repeat = String("Repeat");
        private static readonly Parser<char, string> True = String("true");
        private static readonly Parser<char, string> False = String("false");
        private static readonly Parser<char, string> Add = String("Add");

        private static readonly Parser<char, ConstantBoolNode> BoolConstant = True.Or(False).Select(b => new ConstantBoolNode(b == "true"));

        // Value Operators
        private static readonly Parser<char, string> ExtractNoReplaceOperator = String("->");
        private static readonly Parser<char, string> ExtractWithReplaceOperator = String("~>");
        private static readonly Parser<char, string> UnpackOperator = String("/>");

        // Proposition Operators
        private static readonly Parser<char, string> NotOperator = String("not");
        private static readonly Parser<char, string> OrOperator = String("or");
        private static readonly Parser<char, string> AndOperator = String("and");

        // Function Names
        private static readonly Parser<char, string> ZipFunction = String("Zip");
        private static readonly Parser<char, string> RotateFunction = String("Rotate");
        private static readonly Parser<char, string> GetBoardFunction = String("GetBoard");
        private static readonly Parser<char, string> ConcatFunction = String("Concat");
        private static readonly Parser<char, string> GetPropertyFunction = String("GetProperty");
        private static readonly Parser<char, string> FollowingFunction = String("Following");

        // Proposition function
        private static readonly Parser<char, string> ContainsAtLeastProp = String("ContainsAtLeast");
        private static readonly Parser<char, string> ContainsExactProp = String("ContainsExact");
        private static readonly Parser<char, string> ContainsProp = String("Contains");
        private static readonly Parser<char, string> IntersectsProp = String("Intersects");
        private static readonly Parser<char, string> SubsetProp = String("Subset");
        private static readonly Parser<char, string> EqualsProp = String("Equals");

        
        private static readonly Parser<char, VariableNode> Identifier = Token(char.IsLetter).AtLeastOnceString()
                                                                                            .Select(s => new VariableNode(s));

        private static readonly Parser<char, string> SingleQuoteString = Token(c => c != '\'').ManyString().Between(SingleQuote);
        private static readonly Parser<char, string> DoubleQuoteString = Token(c => c != '"').ManyString().Between(DoubleQuote);
        private static readonly Parser<char, ConstantStringNode> StringConstant = SingleQuoteString.Or(DoubleQuoteString)
                                                                                                   .Select(s => new ConstantStringNode(s));

        private static readonly Parser<char, ConstantIntNode> NumberConstant = Parser.DecimalNum.Select(n => new ConstantIntNode(n));

        // Update with Function results
        private static readonly Parser<char, INode> Value = OneOf(Try(StringConstant.OfType<INode>()),
                                                                  Try(NumberConstant.OfType<INode>()),
                                                                  Try(Rec(() => ValueGroup)),
                                                                  Try(Rec(() => ArrayConstant).OfType<INode>()),
                                                                  Try(Rec(() => Comprehension).OfType<INode>()),
                                                                  Try(Rec(() => Function).OfType<INode>()),
                                                                  Try(Identifier.OfType<INode>()));
        private static readonly Parser<char, INode> ValueGroup = Value.Between(LParen, RParen);

        private static readonly Parser<char, ArrayNode> ArrayConstant = Value.Between(SkipWhitespaces)
                                                                                     .Separated(Comma)
                                                                                     .Between(LBracket, RBracket)
                                                                                     .Select(els => new ArrayNode(els.ToArray()));
        
        // Functions
        private static readonly Parser<char, FunctionNode> Zip = ZipFunction.Then(Value.Between(SkipWhitespaces)
                                                                                       .Separated(Comma)
                                                                                       .Between(LParen, RParen))
                                                                            .Select(els => new FunctionNode(ValueFunction.Zip, els.ToArray()));
        
        private static readonly Parser<char, FunctionNode> Rotate = RotateFunction.Then(Value.Between(SkipWhitespaces)
                                                                                            .Separated(Comma)
                                                                                            .Between(LParen, RParen))
                                                                                  .Select(els => new FunctionNode(ValueFunction.Rotate, els.ToArray()));
        private static readonly Parser<char, FunctionNode> GetBoard = GetBoardFunction.Then(Value.Between(SkipWhitespaces)
                                                                                                 .Separated(Comma)
                                                                                                 .Between(LParen, RParen))
                                                                                      .Select(els => new FunctionNode(ValueFunction.GetBoard, els.ToArray()));
        private static readonly Parser<char, FunctionNode> Concat = ConcatFunction.Then(Value.Between(SkipWhitespaces)
                                                                                             .Separated(Comma)
                                                                                             .Between(LParen, RParen))
                                                                                  .Select(els => new FunctionNode(ValueFunction.Concat, els.ToArray()));
        private static readonly Parser<char, FunctionNode> GetProperty = GetPropertyFunction.Then(Value.Between(SkipWhitespaces)
                                                                                                       .Separated(Comma)
                                                                                                       .Between(LParen, RParen))
                                                                                            .Select(els => new FunctionNode(ValueFunction.GetProperty, els.ToArray()));
        private static readonly Parser<char, FunctionNode> Following = FollowingFunction.Then(Value.Between(SkipWhitespaces)
                                                                                                   .Separated(Comma)
                                                                                                   .Between(LParen, RParen))
                                                                                        .Select(els => new FunctionNode(ValueFunction.Following, els.ToArray()));
        private static readonly Parser<char, FunctionNode> Function = Zip.Or(Rotate).Or(Concat)
                                                                         .Or(OneOf(Try(GetProperty), Try(GetBoard)))
                                                                         .Or(Following);
        
        private static readonly Parser<char, INode> Assignment = Map((i, _, v) => (INode)new AssignNode(i, v), Identifier, Assign.Between(SkipWhitespaces), Value);

        private static readonly Parser<char, INode> ExtractWithReplacement = Map((v, _, i) => (INode)new ExtractNode(v, i, true),
                                                                                        Value, ExtractWithReplaceOperator.Between(SkipWhitespaces), Identifier);
        private static readonly Parser<char, INode> ExtractNoReplacement = Map((v, _, i) => (INode)new ExtractNode(v, i, false),
                                                                                      Value, ExtractNoReplaceOperator.Between(SkipWhitespaces), Identifier);
        private static readonly Parser<char, INode> Unpack = Map((v, _, ids) => (INode)new UnpackNode(v, ids.ToArray()),
                                                                        Value, UnpackOperator.Between(SkipWhitespaces), Identifier.Between(SkipWhitespaces)
                                                                                                         .Separated(Comma));
        private static readonly Parser<char, INode> AddCard = Add.Then(Value.Between(LParen, RParen)).Select<INode>(v => new AddNode(v));
        private static readonly Parser<char, INode> ValueOperator = OneOf(Try(Assignment),
                                                                          Try(ExtractWithReplacement),
                                                                          Try(ExtractNoReplacement),
                                                                          Try(Unpack),
                                                                          Try(AddCard));

        private static readonly Parser<char, Node<bool>> Proposition = BoolConstant.OfType<Node<bool>>()
                                                                           .Or(Rec(() => PropFunction.OfType<Node<bool>>()))
                                                                           .Or(Rec(() => PropositionGroup))
                                                                           .Or(Rec(() => PropOperator.OfType<Node<bool>>()));
        private static readonly Parser<char, Node<bool>> PropositionGroup = Proposition.Between(LParen, RParen);
        
        private static readonly Parser<char, PropOperatorNode> And = Map((p1, _, p2) => new PropOperatorNode(PropositionOperator.And, new[] { p1, p2 }),
                                                                         Proposition, AndOperator.Between(SkipWhitespaces), Proposition);
        private static readonly Parser<char, PropOperatorNode> Or = Map((p1, _, p2) => new PropOperatorNode(PropositionOperator.Or, new[] { p1, p2 }),
                                                                        Proposition, OrOperator.Between(SkipWhitespaces), Proposition);
        private static readonly Parser<char, PropOperatorNode> Not = NotOperator.Then(SkipWhitespaces).Then(Proposition)
                                                                                .Select(p => new PropOperatorNode(PropositionOperator.Not, new Node<bool>[] { p }));
        
        private static readonly Parser<char, PropOperatorNode> PropOperator = OneOf(Try(And), Try(Or), Try(Not));
        
        private static readonly Parser<char, PropFunctionNode> ContainsExact = ContainsExactProp.Then(Value.Between(SkipWhitespaces)
                                                                                                           .Separated(Comma)
                                                                                                           .Between(LParen, RParen))
                                                                                                .Select(els => new PropFunctionNode(PropositionFunction.ContainsExact, els.ToArray()));
        private static readonly Parser<char, PropFunctionNode> ContainsAtLeast = ContainsAtLeastProp.Then(Value.Between(SkipWhitespaces)
                                                                                                               .Separated(Comma)
                                                                                                               .Between(LParen, RParen))
                                                                                                    .Select(els => new PropFunctionNode(PropositionFunction.ContainsAtLeast, els.ToArray()));
        private static readonly Parser<char, PropFunctionNode> Contains = ContainsProp.Then(Value.Between(SkipWhitespaces)
                                                                                                 .Separated(Comma)
                                                                                                 .Between(LParen, RParen))
                                                                                                 .Select(els => new PropFunctionNode(PropositionFunction.Contains, els.ToArray()));
        private static readonly Parser<char, PropFunctionNode> Intersects = IntersectsProp.Then(Value.Between(SkipWhitespaces)
                                                                                                     .Separated(Comma)
                                                                                                     .Between(LParen, RParen))
                                                                                          .Select(els => new PropFunctionNode(PropositionFunction.Intersects, els.ToArray()));
        private static readonly Parser<char, PropFunctionNode> Subset = ContainsExactProp.Then(Value.Between(SkipWhitespaces)
                                                                                                    .Separated(Comma)
                                                                                                    .Between(LParen, RParen))
                                                                                         .Select(els => new PropFunctionNode(PropositionFunction.Subset, els.ToArray()));
        private static readonly Parser<char, PropFunctionNode> Equal = EqualsProp.Then(Value.Between(SkipWhitespaces)
                                                                                            .Separated(Comma)
                                                                                            .Between(LParen, RParen))
                                                                                 .Select(els => new PropFunctionNode(PropositionFunction.Equals, els.ToArray()));
        private static readonly Parser<char, PropFunctionNode> PropFunction = OneOf(Try(ContainsExact),
                                                                                    Try(ContainsAtLeast),
                                                                                    Try(Contains),
                                                                                    Try(Intersects),
                                                                                    Try(Subset),
                                                                                    Try(Equal));

        private static readonly Parser<char, ComprehensionNode> Comprehension = Map((v, _, p) => new ComprehensionNode(v, p),
                                                                                    Value, Where.Between(SkipWhitespaces), Proposition)
                                                                                    .Between(LBracket, RBracket);

        private static readonly Parser<char, INode> Command = OneOf(Try(ValueOperator),
                                                                    Try(Rec(() => RepeatBlock))).Between(SkipWhitespaces);
        private static readonly Parser<char, ArrayNode> CommandBlock = Command.AtLeastOnce().Select(els => new ArrayNode(els.ToArray()));

        private static readonly Parser<char, INode> RepeatBlock = Map((_, n, c) => (INode)new RepeatNode(n, c), Repeat, NumberConstant.Between(SkipWhitespaces),
                                                                             CommandBlock.Between(LBrace.Between(SkipWhitespaces), RBrace.Between(SkipWhitespaces)));

        public ArrayNode Commands { get; private set; }
        public CubeState State { get; private set; }
        public CubeParser(string toParse, Deck deck)
        {
            var a = Comprehension.Parse("[Lands where ContainsExact(GetProperty(X, 'ColorIdentity'), 2)]");
            var b = Proposition.Parse("ContainsExact(GetProperty(X, 'ColorIdentity'), 2)");
            var c = GetProperty.Parse("GetProperty(X, 'ColorIdentity')");
            Result<char, ArrayNode> result = CommandBlock.Parse(toParse);
            if (!result.Success) throw new Exception("Failed to parse the Cube definition");
            this.Commands = result.Value;
            this.State = new CubeState(deck);
        }

        public List<PackCard> MakePack()
        {
            this.State.Pack = new List<PackCard>();
            this.Commands.Evaluate(this.State);
            return this.State.Pack;
        }
    }

    public class PackCard
    {
        public Card Card { get; set; }
        public bool Foil { get; set; }
        public string BoardName { get; set; }

        public object GetProperty(string propertyName)
        {
            if (propertyName == "Foil") return this.Foil;
            else if (propertyName == "Colors") return new ArrayList((this.Card.Colors ?? "").ToList());
            else if (propertyName == "ColorIdentity") return new ArrayList((this.Card.ColorIdentity ?? "").ToList());
            else if (propertyName == "ColorIndicator") return new ArrayList((this.Card.ColorIndicator ?? "").ToList());
            else return typeof(Card).GetProperty(propertyName).GetValue(propertyName);
        }
    }

    public class BoardList : Node<List<PackCard>>
    {
        public string BoardName { get; private set; }
        public BoardList(string boardName)
        {
            this.BoardName = boardName;
        }

        public override List<PackCard> Evaluate(CubeState state)
        {
            return state.Boards[this.BoardName];
        }
    }

    public class BoardConcat : Node<List<PackCard>>
    {
        public Node<List<PackCard>>[] Children { get; private set; }
        public BoardConcat(params Node<List<PackCard>>[] children)
        {
            this.Children = children;
        }

        public override List<PackCard> Evaluate(CubeState state)
        {
            IEnumerable<PackCard> result = new List<PackCard>();
            foreach( Node<List<PackCard>> child in this.Children)
            {
                result.Concat(child.Evaluate(state));
            }
            return result.ToList();
        }
    }

    public class BoardComprehension : Node<List<PackCard>>
    {
        public ComprehensionNode Inner { get; private set; }
        public BoardComprehension(Node<List<PackCard>> inner, Node<bool> filter)
        {
            this.Inner = new ComprehensionNode(inner, filter, true);
        }

        public override List<PackCard> Evaluate(CubeState state)
        {
            ArrayList innerValue = (ArrayList)Inner.Evaluate(state);
            return innerValue.Cast<PackCard>().ToList();
        }
    }

    public class CubeState
    {
        private Deck Deck { get; set; }
        public Dictionary<string, List<PackCard>> Boards { get; set; }

        public Dictionary<string, object> VariableValues { get; set; }

        public List<PackCard> Pack { get; set; }

        public CubeState(Deck deck)
        {
            this.Deck = deck;
            this.Boards = new Dictionary<string, List<PackCard>>();
            foreach(KeyValuePair<string, IDictionary<string, Deck.BoardItem>> pair in deck.Boards)
            {
                IEnumerable<PackCard> cards = new List<PackCard>();
                foreach(Deck.BoardItem bi in pair.Value.Values)
                {
                    List<PackCard> foils = Enumerable.Repeat(new PackCard()
                    {
                        Card = bi.Card,
                        Foil = true,
                        BoardName = pair.Key
                    }, bi.FoilCount).ToList();

                    List<PackCard> normals = Enumerable.Repeat(new PackCard()
                    {
                        Card = bi.Card,
                        Foil = true,
                        BoardName = pair.Key
                    }, bi.NormalCount).ToList();
                    cards = cards.Concat(foils).Concat(normals);
                }
                this.Boards.Add(pair.Key, cards.ToList());
            }
            this.VariableValues = new Dictionary<string, object>();
            this.Pack = new List<PackCard>();
        }

        public CubeState(CubeState other)
        {
            this.Deck = other.Deck;
            this.VariableValues = new Dictionary<string, object>(other.VariableValues);
            this.Pack = other.Pack;
            this.Boards = other.Boards;
        }
    }

    public enum ValueFunction
    {
        Zip,
        Rotate,
        GetBoard,
        Concat,
        GetProperty,
        Following
    }
    public enum PropositionOperator
    {
        Not,
        Or,
        And
    }
    public enum PropositionFunction
    {
        ContainsAtLeast,
        ContainsExact,
        Contains,
        Intersects,
        Subset,
        Equals
    }

    public interface INode
    {
        Type Type { get; }

        object Evaluate(CubeState state);
    }

    public abstract class Node<T> : INode
    {
        public abstract T Evaluate(CubeState state);

        object INode.Evaluate(CubeState state) => Evaluate(state);

        public Type Type => typeof(T);
    }

    public class ConstantNode<T> : Node<T>
    {
        public T Value { get; private set; }
        public ConstantNode(T value)
        {
            this.Value = value;
        }

        public override T Evaluate(CubeState state)
        {
            return this.Value;
        }
    }

    public class VariableNode : INode
    {
        public string Identifier { get; private set; }
        public VariableNode(string identifier)
        {
            /*
            if(identifier == "X" || (identifier.StartsWith("X") && Int32.TryParse(identifier.Substring(1), out int _)))
            {
                throw new Exception($"{identifier} is a reserved Variable");
            }
            */
            this.Identifier = identifier;
            this.Type = null;
        }

        public object Evaluate(CubeState state)
        {
            if (!state.VariableValues.ContainsKey(this.Identifier)) throw new Exception($"Variable {this.Identifier} used before definition");
            return state.VariableValues[this.Identifier];
        }

        public Type Type { get; set; }
    }

    public class AddNode : INode
    {
        public INode Value { get; private set; }
        public AddNode(INode value)
        {
            this.Value = value;
        }

        public object Evaluate(CubeState state)
        {
            object value = this.Value.Evaluate(state);
            if (value is PackCard card) state.Pack.Add(card);
            else throw new Exception("Tried to add a non-card value to the pack");
            return null;
        }

        public Type Type => typeof(void);
    }

    public class ArrayNode : INode
    {
        public INode[] Children { get; private set; }
        public ArrayNode(INode[] children)
        {
            this.Children = children;
            this.Type = null;
        }

        public object Evaluate(CubeState state)
        {
            // CodeReview: Should this assert that all children are of the same type?
            return this.Children.Select(c => c.Evaluate(state)).ToList();
        }

        public Type Type { get; private set; }
    }

    public class FunctionNode : INode
    {
        public INode[] Children { get; private set; }
        public ValueFunction Function { get; private set; }
        public FunctionNode( ValueFunction function, INode[] children )
        {
            this.Function = function;
            this.Children = children;
            this.Type = null; 
            // Check types for each function
            switch (function)
            {
            case ValueFunction.Zip:
            case ValueFunction.Concat:
                if (this.Children.Length <= 1) throw new Exception("Zip/Concat must have at least two arguments");

                Type firstChild = this.Children[0].Type;
                // foreach(INode child in this.Children){
                //     if (child.Type != firstChild) throw new Exception("Zip/Concat must have all arguments be the same type");
                // }
                this.Type = firstChild;
                break;
            case ValueFunction.Rotate:
                if(this.Children.Length != 2) throw new Exception("Invalid number of arguments for Rotate");
                this.Type = this.Children[0].Type;
                break;
            case ValueFunction.GetBoard:
                if(this.Children.Length != 1) throw new Exception("Invalid number of arguments for GetBoard");
                this.Type = typeof(List<Deck.BoardItem>);
                break;
            case ValueFunction.GetProperty:
                if (this.Children.Length != 2) throw new Exception("Invalid number of arguments for GetProperty");
                if (!(this.Children[1] is ConstantStringNode)) throw new Exception("Second argument to GetProperty must be a constant string");
                // Evaluate string and determine type from there
                break;
            case ValueFunction.Following:
                if (this.Children.Length != 2) throw new Exception("Invalid number of arguments for Following");
                // Second type should be IEnumerable<S> where S is the first type
                this.Type = this.Children[1].Type;
                    break;
            }
        }

        public object Evaluate(CubeState state)
        {
            object[] values = this.Children.Select(n => n.Evaluate(state)).ToArray();
            switch(this.Function){
            case ValueFunction.Zip:
                IList[] realValues = values.Select(v => v as IList).ToArray();
                foreach (object tvalue in realValues)
                {
                    if (tvalue is null) throw new Exception("Non-Enumerable type passed to Zip");
                }
                int maxSize = realValues.Select(e => e.Count).Max();
                return Enumerable.Range(0, maxSize)
                                    .Select(i => Enumerable.Range(0, realValues.Length)
                                                        .Select(j => realValues[j][i])
                                                        .ToArray())
                                    .ToList();
            case ValueFunction.Rotate:
                IList realValue = values[0] as IList;
                if (realValue is null) throw new Exception("Non-Enumerable variable passed as first argument to Rotate");
                
                int? tempCount = values[1] as int?;
                if (tempCount is null) throw new Exception("Non-Integer variable passed as second argument to Rotate");
                int count = (int)tempCount;

                return Enumerable.Range(0, realValue.Count).Select(i => new[] { realValue[i], realValue[(i + count) % realValue.Count] }).ToList();
            case ValueFunction.GetBoard:
                string value = values[0] as string;
                if (value is null) throw new Exception("Non-String variable passed as argument to GetBoard");
                return new BoardList(value);
            case ValueFunction.Concat:
                Node<List<PackCard>>[] boards = values.Select(v => v as Node<List<PackCard>>).ToArray();
                bool useBoard = true;
                foreach(Node<List<PackCard>> board in boards)
                {
                    useBoard &= !(board is null);
                }
                if (useBoard) return new BoardConcat(boards);

                IList[] realValues2 = values.Select(v => v as IList).ToArray();
                foreach (object tvalue in realValues2)
                {
                    if (tvalue is null) throw new Exception("Non-Enumerable type passed to Concat");
                }
                IList result = new ArrayList();
                for (int i = 1; i < realValues2.Length; i++)
                {
                    foreach (object o in realValues2[i])
                    {
                        result.Add(o);
                    }
                }
                return result;
            case ValueFunction.GetProperty:
                if (values[0] is PackCard card) return card.GetProperty((string)values[1]);
                else return values[0].GetType().GetProperty((string)values[1]).GetValue(values[0]);
            case ValueFunction.Following:
                IList realValue2 = values[1] as IList;
                if (realValue2 is null) throw new Exception("Non-Enumerable type passed as second argument to Following");
                int index = realValue2.IndexOf(values[0]);
                if (index < 0) throw new Exception("Value was not found in list in Following");
                return realValue2[(index + 1) % realValue2.Count];
            }
            throw new NotImplementedException();
        }

        public Type Type { get; private set; }
    }

    public class AssignNode : INode
    {
        public VariableNode Identifier { get; private set; }
        public INode Value { get; private set; }
        public AssignNode(VariableNode identifier, INode value)
        {
            this.Identifier = identifier;
            this.Value = value;
        }

        public object Evaluate(CubeState state)
        {
            object value = this.Value.Evaluate(state);
            state.VariableValues[this.Identifier.Identifier] = value;
            return value;
        }

        public Type Type => this.Identifier.Type;
    }

    public class ExtractNode : INode
    {
        private static Random rnd = new Random();
        public INode Value { get; private set; }
        public VariableNode Identifier { get; private set; }
        public bool Replacement { get; private set; }
        public ExtractNode(INode value, VariableNode identifier, bool replacement)
        {
            // Check type of value
            this.Value = value;
            this.Identifier = identifier;
            this.Replacement = replacement;
            // Internal type of Value
            this.Type = null;
        }

        public object Evaluate(CubeState state)
        {
            object inner = Value.Evaluate(state);
            if(inner is Node<List<PackCard>> board)
            {
                List<PackCard> cards = board.Evaluate(state);
                if (cards.Count == 0) throw new Exception("Tried to extract from an empty board");
                int ind = rnd.Next(cards.Count);
                PackCard card = cards[ind];
                if (!this.Replacement) state.Boards[card.BoardName].Remove(card);
                state.VariableValues[this.Identifier.Identifier] = card;
                return card;
            }
            IList value = inner as IList;
            if (value is null) throw new Exception("Tried to extract from a Non-Enumerable value");
            if (value.Count == 0) throw new Exception("Tried to extract from an empty List");
            int index = rnd.Next(value.Count);
            object val = value[index];
            if (!this.Replacement) value.RemoveAt(index);

            state.VariableValues[this.Identifier.Identifier] = val;

            return val;
        }

        public Type Type { get; private set; }
    }
    
    public class UnpackNode : INode
    {
        public INode Value { get; private set; }
        public VariableNode[] Children { get; private set; }
        public UnpackNode(INode value, VariableNode[] children)
        {
            // Value must be an array type with known size
            this.Value = value;
            this.Children = children;
        }

        public object Evaluate(CubeState state)
        {
            IList value = this.Value.Evaluate(state) as IList;
            if (value is null) throw new Exception("Tried to unpack from a Non-Enumerable value");
            if (value.Count != this.Children.Length) throw new Exception($"Tried to unpack into {this.Children.Length} variables from Enumerable with {value.Count} elements");
            for(int i=0; i < value.Count; i++)
            {
                state.VariableValues[this.Children[i].Identifier] = value[i];
            }
            return value;
        }

        public Type Type => typeof(void);
    }

    public class PropOperatorNode : Node<bool>
    {
        public Node<bool>[] Children { get; private set; }
        public PropositionOperator Function { get; private set; }
        public PropOperatorNode(PropositionOperator function, Node<bool>[] children)
        {
            this.Function = function;
            this.Children = children;
            // Check types for each function
            switch (function)
            {
                case PropositionOperator.And:
                case PropositionOperator.Or:
                    if (this.Children.Length != 2) throw new Exception("And/Or must have two arguments");
                    break;
                case PropositionOperator.Not:
                    if (this.Children.Length != 1) throw new Exception("Not must only have one argument");
                    break;
                
            }
        }

        public override bool Evaluate(CubeState state)
        {
            bool res1 = this.Children[0].Evaluate(state);
            bool res2;
            switch(this.Function)
            {
                case PropositionOperator.And:
                    res2 = this.Children[1].Evaluate(state);
                    return res1 && res2;
                case PropositionOperator.Or:
                    res2 = this.Children[1].Evaluate(state);
                    return res1 || res2;
                case PropositionOperator.Not:
                    return !res1;
                    
            }
            throw new NotImplementedException();
        }
    }

    public class PropFunctionNode : Node<bool>
    {
        public INode[] Children { get; private set; }
        public PropositionFunction Function { get; private set; }
        public PropFunctionNode(PropositionFunction function, INode[] children)
        {
            this.Function = function;
            this.Children = children;
            // Check types for each function
            switch (function)
            {
            case PropositionFunction.ContainsAtLeast:
            case PropositionFunction.ContainsExact:
            case PropositionFunction.Contains:
                if (children.Length != 2) throw new Exception("ContainsAtLeast/Exact must have exactly 2 arguments");
                break;
            case PropositionFunction.Intersects:
            case PropositionFunction.Subset:
                if (children.Length != 2) throw new Exception("Intersects/Subset must have exactly 2 arguments");
                break;
            case PropositionFunction.Equals:
                if (children.Length != 2) throw new Exception("Equals must have exactly 2 arguments");
                break;
            }
        }

        public override bool Evaluate(CubeState state)
        {
            object[] values = this.Children.Select(n => n.Evaluate(state)).ToArray();
            switch (this.Function)
            {
            case PropositionFunction.ContainsAtLeast:
                int? tCount = values[1] as int?;
                if (tCount is null) throw new Exception("Tried to pass Non-Numeric as second argument to ContainsAtLeast");
                int count = (int)tCount;
                if (values[0] is Node<List<PackCard>> board) return board.Evaluate(state).Count >= count;
                IList value = values[0] as IList;
                if (value is null) throw new Exception("Tried to pass Non-Enumerable as first argument to ContainsAtLeast");
                return value.Count >= count;
            case PropositionFunction.ContainsExact:
                int? tCount2 = values[1] as int?;
                if (tCount2 is null) throw new Exception("Tried to pass Non-Numeric as second argument to ContainsExact");
                int count2 = (int)tCount2;
                if (values[0] is Node<List<PackCard>> board2) return board2.Evaluate(state).Count == count2; 
                IList value2 = values[0] as IList;
                if (value2 is null) throw new Exception("Tried to pass Non-Enumerable as first argument to ContainsExact");
                return value2.Count == count2;
            case PropositionFunction.Contains:
                if (values[0] is Node<List<PackCard>> board3) return board3.Evaluate(state).Contains(values[1]);
                IList value3 = values[0] as IList;
                if (value3 is null) throw new Exception("Tried to pass Non-Enumerable as first argument to Contains");
                return value3.Contains(values[1]);
            case PropositionFunction.Intersects:
                if (values[0] is Node<List<PackCard>> lBoard && values[1] is Node<List<PackCard>> rBoard)
                {
                    return lBoard.Evaluate(state).Intersect(rBoard.Evaluate(state)).Count() > 0;
                }
                IEnumerable left = values[0] as IEnumerable;
                if (left is null) throw new Exception("Tried to pass Non-Enumerable as first argument to Intersects");
                IList right = values[1] as IList;
                if (right is null) throw new Exception("Tried to pass Non-Enumerable as second argument to Intersects");
                return left.Cast<object>().Intersect(right.Cast<object>()).Count() > 0;
            case PropositionFunction.Subset:
                if (values[0] is Node<List<PackCard>> lBoard2 && values[1] is Node<List<PackCard>> rBoard2)
                {
                    List<PackCard> lCards = lBoard2.Evaluate(state);
                    return lCards.Intersect(rBoard2.Evaluate(state)).Count() > lCards.Count;
                }
                IList left1 = values[0] as IList;
                if (left1 is null) throw new Exception("Tried to pass Non-Enumerable as first argument to Subset");
                IList right1 = values[1] as IList;
                if (right1 is null) throw new Exception("Tried to pass Non-Enumerable as second argument to Subset");
                return left1.Cast<object>().Intersect(right1.Cast<object>()).Count() == left1.Count;
            case PropositionFunction.Equals:
                return values[0] == values[1];
            }
            throw new NotImplementedException();
        }
    }

    public class ComprehensionNode : INode
    {
        public INode Value { get; private set; }
        public Node<bool> Criteria { get; private set; }
        public bool IsRecursive { get; private set; }
        public ComprehensionNode(INode value, Node<bool> criteria, bool isRecursive=false)
        {
            this.Value = value;
            this.Criteria = criteria;
            this.Type = null;
            this.IsRecursive = isRecursive;
        }

        public object Evaluate(CubeState state)
        {
            object pVal = this.Value.Evaluate(state);
            if(pVal is Node<List<PackCard>> board && !this.IsRecursive)
            {
                return new BoardComprehension(board, Criteria);
            }
            IList value = pVal as IList;
            
            if (value is null) throw new Exception("Tried to do a comprehension over a Non-Enumerable value");
            return new ArrayList(value.Cast<object>().Where(x =>
            {
                CubeState subState = new CubeState(state);
                subState.VariableValues["X"] = x;
                if (x is IList inner)
                {
                    for (int i = 0; i < inner.Count; i++)
                    {
                        subState.VariableValues["X" + i] = inner[i];
                    }
                }
                return this.Criteria.Evaluate(subState);
            }).ToList());
        }

        public Type Type { get; private set; }
    }

    public class RepeatNode : INode
    {
        public ConstantIntNode Number { get; private set; }
        public ArrayNode Commands { get; set; }
        public RepeatNode(ConstantIntNode number, ArrayNode commands)
        {
            this.Number = number;
            this.Commands = commands;
        }

        public object Evaluate(CubeState state)
        {
            int n = this.Number.Evaluate(state);
            for (int i = 0; i < n; i++)
            {
                this.Commands.Evaluate( state );
            }
            return null;
        }

        public Type Type => typeof(void);
    }
}
