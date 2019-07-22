using System;
using System.Linq.Expressions;

using CB = Couchbase.Lite.Query;

namespace MtSparked.Services.CouchBaseLite {
    public static class CouchbaseExpressionUtils {

        public const int MAX_REDUCTIONS = 10;

        public static CB.IExpression FromLinq(Expression expression) {
            // TODO: Probably need the Param value to properly evaluate some of these things.
            // TODO: Refactor into an ExpressionVisitor?
            if (expression is null) {
                throw new ArgumentNullException(nameof(expression));
            }

            int count = 0;
            while (expression.CanReduce && count < MAX_REDUCTIONS) {
                expression = expression.Reduce();
                count++;
            }

            switch(expression) {
            case BinaryExpression binEx:
                return CouchbaseExpressionUtils.FromBinaryExpression(binEx);
            case ConstantExpression consEx:
                return CouchbaseExpressionUtils.FromConstantExpression(consEx);
            case IndexExpression indEx:
                return CouchbaseExpressionUtils.FronIndexExpression(indEx);
            case LambdaExpression lambEx:
                // TODO: Do we always want to do this?
                return CouchbaseExpressionUtils.FromLinq(lambEx.Body);
            case MemberExpression memEx:
                return CouchbaseExpressionUtils.FromMemberExpression(memEx);
            case MethodCallExpression callEx:
                return CouchbaseExpressionUtils.FromMethodCallExpression(callEx);
            case NewExpression newEx:
                return CouchbaseExpressionUtils.FromNewExpression(newEx);
            case NewArrayExpression newArrEx:
                return CouchbaseExpressionUtils.FromNewArrayExpression(newArrEx);
            case UnaryExpression unEx:
                return CouchbaseExpressionUtils.FromUnaryExpression(unEx);
            case TypeBinaryExpression tbEx:
                return CouchbaseExpressionUtils.FromTypeBinaryExpression(tbEx);
            case MemberInitExpression memInEx:
                return CouchbaseExpressionUtils.FromMemberInitExpression(memInEx);
            case ListInitExpression listInEx:
                return CouchbaseExpressionUtils.FromListInitExpression(listInEx);
            case InvocationExpression invEx:
                return CouchbaseExpressionUtils.FromInvocationExpression(invEx);
            case DefaultExpression defEx:
                return CouchbaseExpressionUtils.FromConstantExpression(Expression.Constant(
                    Expression.Lambda(defEx).Compile().DynamicInvoke(),
                    defEx.Type));
            case ConditionalExpression condEx:
                return CouchbaseExpressionUtils.FromConditionalExpression(condEx);
            default:
                throw new NotImplementedException();
            }
        }

        private static CB.IExpression FromConditionalExpression(ConditionalExpression condEx) => throw new NotImplementedException();
        private static CB.IExpression FromInvocationExpression(InvocationExpression invEx) => throw new NotImplementedException();
        // TODO: Need ElementInit to go with this
        private static CB.IExpression FromListInitExpression(ListInitExpression listInEx) => throw new NotImplementedException();
        private static CB.IExpression FromMemberInitExpression(MemberInitExpression memInEx) => throw new NotImplementedException();
        private static CB.IExpression FromTypeBinaryExpression(TypeBinaryExpression tbEx) => throw new NotImplementedException();
        private static CB.IExpression FromUnaryExpression(UnaryExpression unEx) => throw new NotImplementedException();
        private static CB.IExpression FromNewArrayExpression(NewArrayExpression newArrEx) => throw new NotImplementedException();
        private static CB.IExpression FromNewExpression(NewExpression newEx) => throw new NotImplementedException();
        private static CB.IExpression FromMethodCallExpression(MethodCallExpression callEx) => throw new NotImplementedException();
        private static CB.IExpression FromMemberExpression(MemberExpression memEx) => throw new NotImplementedException();
        private static CB.IExpression FronIndexExpression(IndexExpression indEx) => throw new NotImplementedException();

        private static CB.IExpression FromConstantExpression(ConstantExpression consEx) {
            if(consEx is null) {
                throw new ArgumentNullException(nameof(consEx));
            }

            // TODO: Fill out
            switch(consEx.Type) {
            default:
                throw new NotImplementedException();
            }
        }

        private static CB.IExpression FromBinaryExpression(BinaryExpression binEx) {
            if (binEx is null) {
                throw new ArgumentNullException(nameof(binEx));
            }

            switch (binEx.NodeType) {
            case ExpressionType.Add:
                throw new NotImplementedException();
            case ExpressionType.AddChecked:
                throw new NotImplementedException();
            case ExpressionType.And:
                throw new NotImplementedException();
            case ExpressionType.AndAlso:
                throw new NotImplementedException();
            case ExpressionType.ArrayIndex:
                throw new NotImplementedException();
            case ExpressionType.Coalesce:
                throw new NotImplementedException();
            case ExpressionType.Divide:
                throw new NotImplementedException();
            case ExpressionType.Equal:
                throw new NotImplementedException();
            case ExpressionType.ExclusiveOr:
                throw new NotImplementedException();
            case ExpressionType.GreaterThan:
                throw new NotImplementedException();
            case ExpressionType.GreaterThanOrEqual:
                throw new NotImplementedException();
            case ExpressionType.LeftShift:
                throw new NotImplementedException();
            case ExpressionType.LessThan:
                throw new NotImplementedException();
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.Modulo:
                throw new NotImplementedException();
            case ExpressionType.Multiply:
                throw new NotImplementedException();
            case ExpressionType.MultiplyChecked:
                throw new NotImplementedException();
            case ExpressionType.NotEqual:
                throw new NotImplementedException();
            case ExpressionType.Or:
                throw new NotImplementedException();
            case ExpressionType.OrElse:
                throw new NotImplementedException();
            case ExpressionType.Power:
                throw new NotImplementedException();
            case ExpressionType.RightShift:
                throw new NotImplementedException();
            case ExpressionType.Subtract:
                throw new NotImplementedException();
            case ExpressionType.SubtractChecked:
                throw new NotImplementedException();
            default:
                throw new NotImplementedException();
            }
        }
    }
}
