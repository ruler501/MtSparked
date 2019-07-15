using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Databases {
    public class SortCriteria<T> where T : Model {

        public const string TOTAL = "Total";

        public interface IPropertyTransformation {

            bool Reversible { get; }
            Expression Expression { get; }
            Func<T, object> CompiledExpression { get; }

        }

        public interface IPropertyTransformation<V> : IPropertyTransformation {

            Func<T, V> CompiledExpressionWithType { get; }
            
        }

        public class ConstantPropertyTransformation<V> : IPropertyTransformation<V> {

            public ConstantPropertyTransformation(V value) {
                this.Value = value;
            }

            public V Value { get; }
            public bool Reversible => false;
            public Expression Expression => Expression.Constant(this.Value); 
            public Func<T, object> CompiledExpression => model => this.CompiledExpressionWithType(model);
            public Func<T, V> CompiledExpressionWithType =>
                Expression.Lambda<Func<T, V>>(this.Expression, DataStore<T>.Param).Compile();
        }

        public class PropertyTransformation<U, V> : IPropertyTransformation<V> {

            public MethodInfo PropertyAccessor { get; }
            public Func<U, V> Transformation { get; }
            public bool Reversible { get; }

            public Expression Expression {
                get {
                    Expression propertyAccess = Expression.Property(DataStore<T>.Param, 
                                                                    this.PropertyAccessor);
                    if (this.Transformation is null) {
                        return propertyAccess;
                    } else {
                        return Expression.Call(Expression.Constant(this.Transformation.Target),
                                               this.Transformation.Method,
                                               propertyAccess);
                    }
                }
            }
            public Func<T, object> CompiledExpression => model => this.CompiledExpressionWithType(model);
            public Func<T, V> CompiledExpressionWithType =>
                Expression.Lambda<Func<T, V>>(this.Expression, DataStore<T>.Param).Compile();
    
            public PropertyTransformation(MethodInfo propertyAccessor,
                                          Func<U, V> transformation = null,
                                          bool reversible = true) {
                this.PropertyAccessor = propertyAccessor;
                this.Transformation = transformation;
                this.Reversible = reversible;
            }

        }

        public List<IPropertyTransformation> Criteria { get; }
        public IPropertyTransformation<string> Grouping { get; } = new ConstantPropertyTransformation<string>(TOTAL);

    }
}
