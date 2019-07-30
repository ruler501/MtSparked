using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MtSparked.Interop.Databases;
using Remotion.Linq.Clauses.Expressions;
using CB = Couchbase.Lite.Query;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseExpressionUtils : ExpressionVisitor {

        public const int MAX_REDUCTIONS = 10;

        public static CB.IExpression FromLinq(Expression expression) {
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
            case ParameterExpression _:
            case QuerySourceReferenceExpression _:
                return null;
            default:
                throw new NotImplementedException(expression.ToString());
            }
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private static CB.IExpression FromInvocationExpression(InvocationExpression invEx) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter
        // TODO: Need ElementInit to go with this
#pragma warning disable IDE0060 // Remove unused parameter
        private static CB.IExpression FromListInitExpression(ListInitExpression listInEx) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        private static CB.IExpression FromMemberInitExpression(MemberInitExpression memInEx) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        private static CB.IExpression FromTypeBinaryExpression(TypeBinaryExpression tbEx) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        private static CB.IExpression FromNewArrayExpression(NewArrayExpression newArrEx) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        private static CB.IExpression FromNewExpression(NewExpression newEx) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter

#pragma warning disable IDE0060 // Remove unused parameter
        private static CB.IExpression FromConditionalExpression(ConditionalExpression condEx) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter

        private static CB.IExpression FromMethodCallExpression(MethodCallExpression callEx) {
            if(callEx is null) {
                throw new ArgumentNullException(nameof(callEx));
            }

            CB.IExpression obj = null;
            if (!(callEx.Object is null)) {
                obj = CouchbaseExpressionUtils.FromLinq(callEx.Object);
            }
            MethodInfo method = callEx.Method;
            IEnumerable<CB.IExpression> arguments = callEx.Arguments.Select(ex => CouchbaseExpressionUtils.FromLinq(ex));

            if(callEx.Object is null && method.IsDefined(typeof(ExtensionAttribute), true)) {
                obj = arguments.ElementAt(0);
                arguments = arguments.Skip(1);
            }

            if (method.Name == "Contains") {
                if (arguments.Count() != 1) {
                    throw new ArgumentException("Need exactly 1 argument for Contains");
                } else if (!(callEx.Object is null) && callEx.Object.Type == typeof(string)) {
                    return CB.Function.Contains(obj, arguments.First());
                } else if (method.DeclaringType == typeof(Enumerable)) {
                    return CB.ArrayFunction.Contains(obj, arguments.First());
                }
            } else if (method.Name == "Count") {
                if (arguments.Count() != 0) {
                    throw new ArgumentException("Need exactly 0 arguments for Count");
                }
                return CB.ArrayFunction.Length(obj);
            } else if (method.Name == "ToLower") {
                if (arguments.Count() != 0) {
                    throw new ArgumentException("Need exactly 0 arguments for ToLower");
                }
                return CB.Function.Lower(obj);
            } else if (method.Name == "Trim") {
                if (arguments.Count() != 0) {
                    throw new ArgumentException("Need exactly 0 arguments for Trim");
                }
                return CB.Function.Trim(obj);
            } else if (method.Name == "ToUpper") {
                if (arguments.Count() != 0) {
                    throw new ArgumentException("Need exactly 0 arguments for ToUpper");
                }
                return CB.Function.Upper(obj);
            } else if (method.Name == "Like") {
                if(arguments.Count() != 1) {
                    throw new ArgumentException("Need exactly 1 arguments for Like");
                }
                return obj.Like(arguments.First());
            } else if (method.Name == "Regex") {
                if(arguments.Count() != 1) {
                    throw new ArgumentException("Need exactly 1 argument for Regex");
                }
            }
            // TODO: Figure out Any, AnyAndEvery, Every, and Variable in Couchbase. Seem to be kind of a Where clause for embedded lists?
            // TODO: Figure out Abs, Acos, Asin, Atan, Atan2, Avg, Ceil, Cos, Count, Degrees, Exp(kind of related to Power), Floor, Ln,
            // Log, Ltrim, Max, MillisToString, MillisToUTC, Min, Radians, Round, Rtrim, Sign, Sin, Sqrt, StringToMillis, StringToUTC,
            // Sum, Tan, Trunc, Between, In, Is?, IsNot?, FullTextExpression
            throw new NotImplementedException();
        }

        private static CB.IExpression FromMemberExpression(MemberExpression memEx) {
            if (memEx is null) {
                throw new ArgumentNullException(nameof(memEx));
            }

            CB.IExpression obj = CouchbaseExpressionUtils.FromLinq(memEx.Expression);
            MemberInfo member = memEx.Member;

            if(member.Name == "Length") {
                if (memEx.Expression.Type == typeof(string)) {
                    return CB.Function.Length(obj);
                } else {
                    return CB.ArrayFunction.Length(obj);
                }
            } else if(member.MemberType == MemberTypes.Property) {
                // We assume that if the expression is a param with name model(referenced from DataStore)
                if ((memEx.Expression is ParameterExpression pamEx && pamEx.Name == DataStore.ParamName)
                     || memEx.Expression is QuerySourceReferenceExpression) {
                    return CB.Expression.Property(member.Name);
                } else {
                    throw new NotImplementedException("Access to non Member property");
                }
            } else {
                throw new ArgumentException("Member: " + member.ToString());
            }
        }

        private static CB.IExpression FronIndexExpression(IndexExpression indEx) {
            if (indEx is null) {
                throw new ArgumentNullException(nameof(indEx));
            }
            if(indEx.Arguments.Count != 1) {
                throw new ArgumentException("Must only be one index argument");
            }

            if (indEx.Object is MemberExpression memEx
                 && memEx.Expression is ParameterExpression pamEx
                 && pamEx.Name == DataStore.ParamName
                 && indEx.Arguments.First() is ConstantExpression conEx) {
                return CB.Expression.Property($"{memEx.Member.Name}[{conEx.Value}]");
            }

            throw new NotImplementedException();
        }

        private static CB.IExpression FromUnaryExpression(UnaryExpression unEx) {
            if (unEx is null) {
                throw new ArgumentNullException(nameof(unEx));
            }

            CB.IExpression body = CouchbaseExpressionUtils.FromLinq(unEx.Operand);
            switch (unEx.NodeType) {
            case ExpressionType.Not:
                return CB.Expression.Negated(body);
            case ExpressionType.UnaryPlus:
                // TODO: Account for possible type specific implementations?
                return body;
            case ExpressionType.Negate:
            case ExpressionType.NegateChecked:
            case ExpressionType.Quote:
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
            case ExpressionType.TypeAs:
            case ExpressionType.ArrayLength:
            default:
                throw new NotImplementedException("Unary: " + Enum.GetName(typeof(ExpressionType), unEx.NodeType));
            }
        }

        private static CB.IExpression FromConstantExpression(ConstantExpression consEx) {
            if(consEx is null) {
                throw new ArgumentNullException(nameof(consEx));
            }

            // TODO: Convert to dict or some nice way to dispatch.
            if (consEx.Type == typeof(IList)) {
                return CB.Expression.Array((IList)consEx.Value);
            }
            else if (consEx.Type == typeof(bool)) {
                return CB.Expression.Boolean((bool)consEx.Value);
            }
            else if (consEx.Type == typeof(DateTimeOffset)) {
                return CB.Expression.Date((DateTimeOffset)consEx.Value);
            }
            else if (consEx.Type == typeof(IDictionary<string, object>)) {
                return CB.Expression.Dictionary((IDictionary<string, object>)consEx.Value);
            }
            else if (consEx.Type == typeof(double)) {
                return CB.Expression.Double((double)consEx.Value);
            }
            else if (consEx.Type == typeof(float)) {
                return CB.Expression.Float((float)consEx.Value);
            }
            else if (consEx.Type == typeof(int)) {
                return CB.Expression.Int((int)consEx.Value);
            }
            else if (consEx.Type == typeof(long)) {
                return CB.Expression.Long((long)consEx.Value);
            }
            else if (consEx.Type == typeof(string)) {
                return CB.Expression.String((string)consEx.Value);
            } else {
                System.Diagnostics.Debug.WriteLine("Constant: " + consEx.Type.Name);
                return CB.Expression.Value(consEx.Value);
            }
        }

        private static CB.IExpression FromBinaryExpression(BinaryExpression binEx) {
            if (binEx is null) {
                throw new ArgumentNullException(nameof(binEx));
            }

            CB.IExpression left = CouchbaseExpressionUtils.FromLinq(binEx.Left);
            CB.IExpression right = CouchbaseExpressionUtils.FromLinq(binEx.Right);

            switch (binEx.NodeType) {
            case ExpressionType.Power:
                return CB.Function.Power(left, right);
            case ExpressionType.Add:
            case ExpressionType.AddChecked:
                return left.Add(right);
            case ExpressionType.And:
            case ExpressionType.AndAlso:
                return left.And(right);
            case ExpressionType.Divide:
                return left.Divide(right);
            case ExpressionType.Equal:
                if(binEx.Right is ConstantExpression conEx && conEx.Value is null) {
                    return left.IsNullOrMissing();
                } else if(binEx.Left is ConstantExpression conExL && conExL.Value is null) {
                    return right.IsNullOrMissing();
                }
                return left.EqualTo(right);
            case ExpressionType.GreaterThan:
                return left.GreaterThan(right);
            case ExpressionType.GreaterThanOrEqual:
                return left.GreaterThanOrEqualTo(right);
            case ExpressionType.LessThan:
                return left.LessThan(right);
            case ExpressionType.LessThanOrEqual:
                return left.LessThan(right);
            case ExpressionType.Modulo:
                return left.Modulo(right);
            case ExpressionType.Multiply:
            case ExpressionType.MultiplyChecked:
                return left.Multiply(right);
            case ExpressionType.NotEqual:
                if (binEx.Right is ConstantExpression conExRNe && conExRNe.Value is null) {
                    return left.NotNullOrMissing();
                }
                else if (binEx.Left is ConstantExpression conExLNe && conExLNe.Value is null) {
                    return right.NotNullOrMissing();
                }
                return left.NotEqualTo(right);
            case ExpressionType.Or:
            case ExpressionType.OrElse:
                return left.Or(right);
            case ExpressionType.Subtract:
            case ExpressionType.SubtractChecked:
                return left.Subtract(right);
            case ExpressionType.ArrayIndex:
            case ExpressionType.Coalesce:
            case ExpressionType.ExclusiveOr:
            case ExpressionType.LeftShift:
            case ExpressionType.RightShift:
            default:
                throw new NotImplementedException();
            }
        }
    }
}
