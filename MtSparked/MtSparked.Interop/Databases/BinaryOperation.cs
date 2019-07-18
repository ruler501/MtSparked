using System;
using System.ComponentModel;
using System.Linq.Expressions;
using MtSparked.Interop.Models;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Databases {
    public sealed class BinaryOperation
            : EnumSingleton<BinaryOperation, BinaryOperation.BinaryOperator>,
              IBinaryExpressionTransformation {

        public enum BinaryOperator {
            [Description("Equals")]
            EQUALS,
            [Description("Contains")]
            CONTAINS,
            [Description("Like")]
            LIKE,
            [Description("Starts With")]
            STARTS_WITH,
            [Description("Ends With")]
            ENDS_WITH,
            [Description("Matches Regex")]
            MATCHES_REGEX,
            [Description("Contains Regex")]
            CONTAINS_REGEX,
            [Description("Less Than")]
            LESS_THAN,
            [Description("Greater Than")]
            GREATER_THAN,
            [Description("Less Than or Equal to")]
            LESS_THAN_OR_EQUAL,
            [Description("Greater Than or Equal to")]
            GREATER_THAN_OR_EQUAL
        }

        static BinaryOperation() {
            _ = new BinaryOperation(BinaryOperator.EQUALS,
                                   apply: (l, r) => Expression.Equal(l, r));
            _ = new BinaryOperation(BinaryOperator.CONTAINS,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.LIKE,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.STARTS_WITH,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.ENDS_WITH,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.MATCHES_REGEX,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.CONTAINS_REGEX,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.LESS_THAN,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.GREATER_THAN,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.LESS_THAN_OR_EQUAL,
                                    apply: (l, r) => throw new NotImplementedException());
            _ = new BinaryOperation(BinaryOperator.GREATER_THAN_OR_EQUAL,
                                    apply: (l, r) => throw new NotImplementedException());
            EnumSingleton<BinaryOperation, BinaryOperator>.Close();
        }

        private BinaryOperation(BinaryOperator value,
                                Func<Expression, Expression, Expression> apply)
                : base(value) {
            this.Apply = apply;
        }

        public BinaryOperator Op { get; }

        public Func<Expression, Expression, Expression> Apply { get; }

        public Expression ApplyWith<T>(string field,
                                       object value) where T : Model {
            // TODO: Implement type checking for BinaryOperations
            Expression left = Expression.Property(DataStore<T>.Param, field);
            Expression right = Expression.Constant(value);
            return this.Apply(left, right);
        }

    }
}
