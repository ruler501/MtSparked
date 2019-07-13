using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using System;
using Pidgin;
using System.Linq;
using System.Collections.Generic;
using MtSparked.Interop.Models;
using ConstantIntNode = MtSparked.Core.Cube.Parser.ConstantNode<int>;
using ConstantStringNode = MtSparked.Core.Cube.Parser.ConstantNode<string>;
using ConstantBoolNode = MtSparked.Core.Cube.Parser.ConstantNode<bool>;

namespace MtSparked.Core.Cube.Parser {
    public class CubeParser {

        // Special symbols
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
        private static readonly Parser<char, string> Add = String("Add");
        private static readonly Parser<char, string> True = String("true");
        private static readonly Parser<char, string> False = String("false");
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

        
        // Variable
        private static readonly Parser<char, VariableNode> Identifier = Token(System.Char.IsLetter).AtLeastOnceString()
                                                                                            .Select(s => new VariableNode(s));

        // Strings
        private static readonly Parser<char, string> SingleQuoteString = Token(c => c != '\'').ManyString().Between(SingleQuote);
        private static readonly Parser<char, string> DoubleQuoteString = Token(c => c != '"').ManyString().Between(DoubleQuote);
        private static readonly Parser<char, ConstantStringNode> StringConstant = SingleQuoteString.Or(DoubleQuoteString)
                                                                                                   .Select(s => new ConstantStringNode(s));

        // Integers
        private static readonly Parser<char, ConstantIntNode> NumberConstant = DecimalNum.Select(n => new ConstantIntNode(n));

        // Value
        private static readonly Parser<char, ICubeParserNode> Value = OneOf(Try(StringConstant.OfType<ICubeParserNode>()),
                                                                  Try(NumberConstant.OfType<ICubeParserNode>()),
                                                                  Try(Rec(() => ValueGroup)),
                                                                  Try(Rec(() => ArrayConstant).OfType<ICubeParserNode>()),
                                                                  Try(Rec(() => Comprehension).OfType<ICubeParserNode>()),
                                                                  Try(Rec(() => Function).OfType<ICubeParserNode>()),
                                                                  Try(Identifier.OfType<ICubeParserNode>()));
        private static readonly Parser<char, ICubeParserNode> ValueGroup = Value.Between(LParen, RParen);

        // Array
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
        
        // Operations
        private static readonly Parser<char, ICubeParserNode> Assignment = Map((i, _, v) => (ICubeParserNode)new AssignNode(i, v), Identifier, Assign.Between(SkipWhitespaces), Value);

        private static readonly Parser<char, ICubeParserNode> ExtractWithReplacement = Map((v, _, i) => (ICubeParserNode)new ExtractNode(v, i, true),
                                                                                        Value, ExtractWithReplaceOperator.Between(SkipWhitespaces), Identifier);
        private static readonly Parser<char, ICubeParserNode> ExtractNoReplacement = Map((v, _, i) => (ICubeParserNode)new ExtractNode(v, i, false),
                                                                                      Value, ExtractNoReplaceOperator.Between(SkipWhitespaces), Identifier);
        private static readonly Parser<char, ICubeParserNode> Unpack = Map((v, _, ids) => (ICubeParserNode)new UnpackNode(v, ids.ToArray()),
                                                                        Value, UnpackOperator.Between(SkipWhitespaces), Identifier.Between(SkipWhitespaces)
                                                                                                         .Separated(Comma));
        private static readonly Parser<char, ICubeParserNode> AddCard = Add.Then(Value.Between(LParen, RParen)).Select<ICubeParserNode>(v => new AddNode(v));
        private static readonly Parser<char, ICubeParserNode> ValueOperations = OneOf(Try(Assignment),
                                                                          Try(ExtractWithReplacement),
                                                                          Try(ExtractNoReplacement),
                                                                          Try(Unpack),
                                                                          Try(AddCard));

        // Proposition
        private static readonly Parser<char, CubeParserNode<bool>> Proposition = BoolConstant.OfType<CubeParserNode<bool>>()
                                                                           .Or(Rec(() => PropFunction.OfType<CubeParserNode<bool>>()))
                                                                           .Or(Rec(() => PropositionGroup))
                                                                           .Or(Rec(() => PropOperator.OfType<CubeParserNode<bool>>()));
        private static readonly Parser<char, CubeParserNode<bool>> PropositionGroup = Proposition.Between(LParen, RParen);
        
        // Proposition Operators
        private static readonly Parser<char, PropOperatorNode> And = Map((p1, _, p2) => new PropOperatorNode(PropositionOperator.And, new[] { p1, p2 }),
                                                                         Proposition, AndOperator.Between(SkipWhitespaces), Proposition);
        private static readonly Parser<char, PropOperatorNode> Or = Map((p1, _, p2) => new PropOperatorNode(PropositionOperator.Or, new[] { p1, p2 }),
                                                                        Proposition, OrOperator.Between(SkipWhitespaces), Proposition);
        private static readonly Parser<char, PropOperatorNode> Not = NotOperator.Then(SkipWhitespaces).Then(Proposition)
                                                                                .Select(p => new PropOperatorNode(PropositionOperator.Not, new CubeParserNode<bool>[] { p }));
        
        private static readonly Parser<char, PropOperatorNode> PropOperator = OneOf(Try(And), Try(Or), Try(Not));
        
        // Proposition Functions
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

        // Comprehension
        private static readonly Parser<char, ComprehensionNode> Comprehension = Map((v, _, p) => new ComprehensionNode(v, p),
                                                                                    Value, Where.Between(SkipWhitespaces), Proposition)
                                                                                    .Between(LBracket, RBracket);

        // Commands
        private static readonly Parser<char, ICubeParserNode> Command = OneOf(Try(ValueOperations),
                                                                    Try(Rec(() => RepeatBlock))).Between(SkipWhitespaces);
        private static readonly Parser<char, ArrayNode> CommandBlock = Command.AtLeastOnce().Select(els => new ArrayNode(els.ToArray()));

        // Repeats
        private static readonly Parser<char, ICubeParserNode> RepeatBlock = Map((_, n, c) => (ICubeParserNode)new RepeatNode(n, c), Repeat, NumberConstant.Between(SkipWhitespaces),
                                                                             CommandBlock.Between(LBrace.Between(SkipWhitespaces), RBrace.Between(SkipWhitespaces)));

        public ArrayNode Commands { get; private set; }
        public CubeState State { get; private set; }

        public CubeParser(string toParse)
            : this(toParse, null)
        {}

        public CubeParser(string toParse, Deck deck) {
            Result<char, ArrayNode> result = CommandBlock.Parse(toParse);
            if (!result.Success) {
                throw new ArgumentException(nameof(toParse), "Failed to parse the Cube definition");
            }
            this.Commands = result.Value;
            if (!(deck is null)) {
                this.State = new CubeState(deck);
            }
        }

        public List<PackCard> MakePack() {
            this.State.Pack = new List<PackCard>();
            _ = this.Commands.Evaluate(this.State);
            return this.State.Pack;
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
}
