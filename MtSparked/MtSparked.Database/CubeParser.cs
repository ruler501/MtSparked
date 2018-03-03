using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using System;
using Pidgin;
using System.Collections.Immutable;
using System.Linq;
using System.Collections.Generic;
using MtSparked.Models;

namespace MtSparked.Database
{
    public static class CubeParser
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

        // Keyword
        private static readonly Parser<char, string> Where = String("where");
        private static readonly Parser<char, string> Repeat = String("Repeat");
        private static readonly Parser<char, string> True = String("true");
        private static readonly Parser<char, string> False = String("false");
        private static readonly Parser<char, string> Add = String("Add");

        private static readonly Parser<char, Node<bool>> BoolConstant = True.Or(False).Select<Node<bool>>(b => new ConstantBoolNode(b == "true"));

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
        private static readonly Parser<char, string> IntersectsProp = String("Intersects");
        private static readonly Parser<char, string> SubsetProp = String("Subset");
        private static readonly Parser<char, string> EqualsProp = String("Equals");

        private static readonly Parser<char, VariableNode> Identifier = Token(char.IsLetter).ManyString()
                                                                                            .Select(s => new VariableNode(s));

        private static readonly Parser<char, string> SingleQuoteString = Token((c => c != '\'')).ManyString().Between(SingleQuote);
        private static readonly Parser<char, string> DoubleQuoteString = Token((c => c != '"')).ManyString().Between(DoubleQuote);
        private static readonly Parser<char, Node<string>> StringConstant = SingleQuoteString.Or(DoubleQuoteString)
                                                                                             .Select<Node<string>>(s => new ConstantStringNode(s));

        private static readonly Parser<char, ConstantIntNode> NumberConstant = Parser.DecimalNum.Select(n => new ConstantIntNode(n));

        // Update with Function results
        private static readonly Parser<char, Node<object>> Value = StringConstant.OfType<Node<object>>()
                                                                                 .Or(NumberConstant.OfType<Node<object>>())
                                                                                 .Or(Rec(() => ValueGroup))
                                                                                 .Or(Rec(() => ArrayConstant).OfType<Node<object>>())
                                                                                 .Or(Rec(() => Comprehension).OfType<Node<object>>())
                                                                                 .Or(Rec(() => Function).OfType<Node<object>>())
                                                                                 .Or(Identifier.OfType<Node<object>>());
        private static readonly Parser<char, Node<object>> ValueGroup = Value.Between(LParen, RParen);

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
        private static readonly Parser<char, FunctionNode> GetBoard = GetBoard.Then(Value.Between(SkipWhitespaces)
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
        private static readonly Parser<char, FunctionNode> Function = Zip.Or(Rotate).Or(GetBoard).Or(Concat).Or(GetProperty).Or(Following);

        private static readonly Parser<char, Node<object>> Assignment = Map((i, _, v) => (Node<object>)new AssignNode(i, v), Identifier, Assign, Value);

        private static readonly Parser<char, Node<object>> ExtractWithReplacement = Map((v, _, i) => (Node<object>)new ExtractNode(v, i, true),
                                                                                        Value, ExtractWithReplaceOperator, Identifier);
        private static readonly Parser<char, Node<object>> ExtractNoReplacement = Map((v, _, i) => (Node<object>)new ExtractNode(v, i, false),
                                                                                      Value, ExtractNoReplaceOperator, Identifier);
        private static readonly Parser<char, Node<object>> Unpack = Map((v, _, ids) => (Node<object>)new UnpackNode(v, ids.ToArray()),
                                                                        Value, UnpackOperator.Between(SkipWhitespaces), Identifier.Between(SkipWhitespaces)
                                                                                                         .Separated(Comma));
        private static readonly Parser<char, Node<object>> AddCard = Add.Then(Value.Between(LParen, RParen)).Select<Node<object>>(v => new AddNode(v));
        private static readonly Parser<char, Node<object>> ValueOperators = Assignment.Or(ExtractWithReplacement).Or(ExtractNoReplacement).Or(Unpack).Or(AddCard);
        
        private static readonly Parser<char, Node<bool>> Proposition = BoolConstant
                                                                           .Or(PropFunction.OfType<Node<bool>>())
                                                                           .Or(Rec(() => PropositionGroup))
                                                                           .Or(PropOperator.OfType<Node<bool>>());
        private static readonly Parser<char, Node<bool>> PropositionGroup = Proposition.Between(LParen, RParen);

        private static readonly Parser<char, PropOperatorNode> And = Map((p1, _, p2) => new PropOperatorNode(PropositionOperator.And, new[] { p1, p2 }),
                                                                         Proposition, AndOperator.Between(SkipWhitespaces), Proposition);
        private static readonly Parser<char, PropOperatorNode> Or = Map((p1, _, p2) => new PropOperatorNode(PropositionOperator.Or, new[] { p1, p2 }),
                                                                        Proposition, OrOperator.Between(SkipWhitespaces), Proposition);
        private static readonly Parser<char, PropOperatorNode> Not = NotOperator.Then(SkipWhitespaces).Then(Proposition)
                                                                                .Select(p => new PropOperatorNode(PropositionOperator.Not, new Node<bool>[] { p }));
        private static readonly Parser<char, PropOperatorNode> PropOperator = And.Or(Or).Or(Not);

        private static readonly Parser<char, PropFunctionNode> ContainsExact = ContainsExactProp.Then(Value.Between(SkipWhitespaces)
                                                                                                           .Separated(Comma)
                                                                                                           .Between(LParen, RParen))
                                                                                                .Select(els => new PropFunctionNode(PropositionFunction.ContainsExact, els.ToArray()));
        private static readonly Parser<char, PropFunctionNode> ContainsAtLeast = ContainsAtLeastProp.Then(Value.Between(SkipWhitespaces)
                                                                                                               .Separated(Comma)
                                                                                                               .Between(LParen, RParen))
                                                                                                    .Select(els => new PropFunctionNode(PropositionFunction.ContainsAtLeast, els.ToArray()));
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
        private static readonly Parser<char, PropFunctionNode> PropFunction = ContainsExact.Or(ContainsAtLeast).Or(Intersects).Or(Subset).Or(Equal);

        private static readonly Parser<char, ComprehensionNode> Comprehension = Map((v, _, p) => new ComprehensionNode(v, p), Value, Where, Proposition)
                                                                                    .Between(LBracket, RBracket);

        private static readonly Parser<char, Node<object>> Command = ValueOperators.Or(Rec(() => RepeatBlock));
        private static readonly Parser<char, ArrayNode> CommandBlock = Command.Separated(SkipWhitespaces).Select(els => new ArrayNode(els.ToArray()));

        private static readonly Parser<char, Node<object>> RepeatBlock = Map((_, n, c) => (Node<object>)new RepeatNode(n, c), Repeat, NumberConstant,
                                                                             CommandBlock.Between(LBrace, RBrace));

        public static Result<char, ArrayNode> Parse(string toParse)
        {
            return CommandBlock.Parse(toParse);
        }
    }

    public interface Node
    {
        Type Type { get; }
    }

    public interface Node<out T> : Node
    {
        T Evaluate(object state);
    }

    public class ConstantIntNode : Node<int>
    {
        public int Value { get; private set; }
        public ConstantIntNode(int value)
        {
            this.Value = Value;
        }

        public int Evaluate(object state)
        {
            return this.Value;
        }

        public Type Type => typeof(int);
    }

    public class ConstantStringNode : Node<string>
    {
        public string Value { get; private set; }
        public ConstantStringNode(string value)
        {
            this.Value = Value;
        }

        public string Evaluate(object state)
        {
            return this.Value;
        }

        public Type Type => typeof(string);
    }

    public class ConstantBoolNode : Node<bool>
    {
        public bool Value { get; private set; }
        public ConstantBoolNode(bool value)
        {
            this.Value = value;
        }

        public bool Evaluate(object state)
        {
            return this.Value;
        }

        public Type Type => typeof(bool);
    }

    public class VariableNode : Node<object>
    {
        public string Identifier { get; private set; }
        public VariableNode(string identifier)
        {
            this.Identifier = identifier;
            this.Type = null;
        }

        public object Evaluate(object state)
        {
            throw new NotImplementedException();
        }

        public Type Type { get; set; }
    }

    public class AddNode : Node<object>
    {
        public Node<object> Value { get; private set; }
        public AddNode(Node<object> value)
        {
            this.Value = value;
        }

        public object Evaluate(object state)
        {
            throw new NotImplementedException();
        }

        public Type Type => typeof(void);
    }

    public class ArrayNode : Node<object[]>
    {
        public Node<object>[] Children { get; private set; }
        public ArrayNode(Node<object>[] children)
        {
            this.Children = children;
            this.Type = null;
        }

        public object[] Evaluate(object state)
        {
            return this.Children.Select(c => c.Evaluate(state)).ToArray();
        }

        public Type Type { get; private set; }
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
        Intersects,
        Subset,
        Equals
    }

    public class FunctionNode : Node<object>
    {
        public Node<object>[] Children { get; private set; }
        public ValueFunction Function { get; private set; }
        public FunctionNode( ValueFunction function, Node<object>[] children )
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

                    foreach(Node<object> child in this.Children){
                        if (child.Type != firstChild) throw new Exception("Zip/Concat must have all arguments be the same type");
                    }
                    this.Type = firstChild;
                    break;
                case ValueFunction.Rotate:
                    if(this.Children.Length != 2) throw new Exception("Invalid number of arguments for Rotate");
                    this.Type = this.Children[0].Type;
                    break;
                case ValueFunction.GetBoard:
                    if(this.Children.Length != 1) throw new Exception("Invalid number of arguments for GetBoard");
                    this.Type = typeof(List<Card>);
                    break;
                case ValueFunction.GetProperty:
                    if (this.Children.Length != 2) throw new Exception("Invalid number of arguments for GetProperty");
                    if (!(this.Children[1] is ConstantStringNode)) throw new Exception("Second argument to GetProperty must be a constant string");
                    // Evaluate string and determine type from there
                    break;
                case ValueFunction.Following:
                    if (this.Children.Length != 2) throw new Exception("Invalid number of arguments for Following");
                    // First type should be List<S> or S[] where S is the second type
                    break;
            }
        }

        public object Evaluate(object state)
        {
            // Implement them
            switch(this.Function){
                case ValueFunction.Zip:
                    break;
                case ValueFunction.Rotate:
                    break;
                case ValueFunction.GetBoard:
                    break;
                case ValueFunction.Concat:
                    break;
                case ValueFunction.GetProperty:
                    break;
                case ValueFunction.Following:
                    break;
            }
            throw new NotImplementedException();
        }

        public Type Type { get; private set; }
    }

    public class AssignNode : Node<object>
    {
        public VariableNode Identifier { get; private set; }
        public Node<object> Value { get; private set; }
        public AssignNode(VariableNode identifier, Node<object> value)
        {
            this.Identifier = identifier;
            this.Value = value;
        }

        public object Evaluate(object state)
        {
            object value = this.Value.Evaluate(state);
            // Store variable value/define
            return value;
            throw new NotImplementedException();
        }

        Type type;
        public Type Type => this.Identifier.Type;
    }

    public class ExtractNode : Node<object>
    {
        public Node<object> Value { get; private set; }
        public VariableNode Identifier { get; private set; }
        public bool Replacement { get; private set; }
        public ExtractNode(Node<object> value, VariableNode identifier, bool replacement)
        {
            // Check type of value
            this.Value = value;
            this.Identifier = identifier;
            this.Replacement = replacement;
            // Internal type of Value
            this.Type = null;
        }

        public object Evaluate(object state)
        {
            object val = Value.Evaluate(state);
            // Need to reflect and find type then randomly grab an element
            throw new NotImplementedException();
            return val;
        }

        public Type Type { get; private set; }
    }
    
    public class UnpackNode : Node<object>
    {
        public Node<object> Value { get; private set; }
        public VariableNode[] Children { get; private set; }
        public UnpackNode(Node<object> value, VariableNode[] children)
        {
            // Value must be an array type with known size
            this.Value = value;
            this.Children = children;
        }

        public object Evaluate(object state)
        {
            // Size of Children must be equal to size of value
            throw new NotImplementedException();
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

        public bool Evaluate(object state)
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

        public Type Type => typeof(bool);
    }

    public class PropFunctionNode : Node<bool>
    {
        public Node<object>[] Children { get; private set; }
        public PropositionFunction Function { get; private set; }
        public PropFunctionNode(PropositionFunction function, Node<object>[] children)
        {
            this.Function = function;
            this.Children = children;
            // Check types for each function
            switch (function)
            {
                case PropositionFunction.ContainsAtLeast:
                case PropositionFunction.ContainsExact:
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

        public bool Evaluate(object state)
        {
            switch (this.Function)
            {
                case PropositionFunction.ContainsAtLeast:
                    break;
                case PropositionFunction.ContainsExact:
                    break;
                case PropositionFunction.Intersects:
                    break;
                case PropositionFunction.Subset:
                    break;
                case PropositionFunction.Equals:
                    break;

            }
            throw new NotImplementedException();
        }

        public Type Type => typeof(bool);
    }

    public class ComprehensionNode : Node<List<object>>
    {
        public Node<object> Value { get; private set; }
        public Node<bool> Criteria { get; private set; }
        public ComprehensionNode(Node<object> value, Node<bool> criteria)
        {
            this.Value = value;
            this.Criteria = criteria;
            this.Type = null;  
        }

        public List<object> Evaluate(object state)
        {
            throw new NotImplementedException();
        }

        public Type Type { get; private set; }
    }

    public class RepeatNode : Node<object>
    {
        public ConstantIntNode Number { get; private set; }
        public ArrayNode Commands { get; set; }
        public RepeatNode(ConstantIntNode number, ArrayNode commands)
        {
            this.Number = number;
            this.Commands = commands;
        }

        public object Evaluate(object state)
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
