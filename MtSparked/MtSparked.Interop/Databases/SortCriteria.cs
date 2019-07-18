using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Databases {
    public interface IPropertyTransformation {
        bool Reversible { get; }
        Expression Expression { get; }

    }

    public interface ISortCriteria {

        List<IPropertyTransformation> UntypedCriteria { get; }
        IPropertyTransformation UntypedGrouping { get; }
    }

    public class SortCriteria<T> : ISortCriteria where T : Model {

        public const string TOTAL = "Total";

        public interface IPropertyTransformation : Databases.IPropertyTransformation {

            Func<T, object> UntypedCompiledLambda { get; }

        }

        public interface IPropertyTransformation<V> : IPropertyTransformation {

            Expression<Func<T, V>> LambdaExpression { get; }
            Func<T, V> CompiledLambda { get; }
            
        }

        public abstract class AbstractPropertyTransformation<V> : IPropertyTransformation<V> {

            protected AbstractPropertyTransformation(bool reversible,
                                                     Expression expression) {
                this.Reversible = reversible;
                this.Expression = expression;
            }

            public bool Reversible { get; }
            public Expression Expression { get; }

            public Expression<Func<T, V>> LambdaExpression
                => Expression.Lambda<Func<T, V>>(this.Expression, DataStore<T>.Param);
            public Func<T, V> CompiledLambda => this.LambdaExpression.Compile();
            public Func<T, object> UntypedCompiledLambda => model => this.CompiledLambda(model);

        }

        public class ConstantPropertyTransformation<V> : AbstractPropertyTransformation<V> {

            public ConstantPropertyTransformation(V value)
                    : base(false, Expression.Constant(value)) {
            }

            public V Value { get; }

        }

        public class PropertyTransformation<U, V> : AbstractPropertyTransformation<V> {

            private static Expression CalculateExpression(MethodInfo accessor,
                                                          Func<U, V> transformation) {
                Expression propertyAccess = Expression.Property(DataStore<T>.Param,
                                                                accessor);
                if (transformation is null) {
                    return propertyAccess;
                } else {
                    return Expression.Call(Expression.Constant(transformation.Target),
                                                               transformation.Method,
                                                               propertyAccess);
                }
            }

            public MethodInfo PropertyAccessor { get; }
            public Func<U, V> Transformation { get; }
    
            public PropertyTransformation(MethodInfo propertyAccessor,
                                          Func<U, V> transformation = null,
                                          bool reversible = true)
                    : base(reversible,
                           PropertyTransformation<U, V>.CalculateExpression(propertyAccessor, transformation)) {
                this.PropertyAccessor = propertyAccessor;
                this.Transformation = transformation;
            }

        }

        public List<IPropertyTransformation<T>> Criteria { get; }
        public IPropertyTransformation<string> Grouping { get; private set; } = new ConstantPropertyTransformation<string>(TOTAL);
        public List<Databases.IPropertyTransformation> UntypedCriteria
            => this.Criteria.Cast<Databases.IPropertyTransformation>().ToList();
        public Databases.IPropertyTransformation UntypedGrouping => this.Grouping;

    }
}
