using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Databases {
    public class ListQuery<T> : DataStore<T>.IQuery where T : Model {

        private Expression BuiltExpression { get; set; }

        public Connector Connector { get; }

        public SortCriteria<T> SortCriteria { get;  }

        public IEnumerable<T> Domain { get; }
        public DataStore<T> ToDataStore() =>
            new DataStore<T>(this.Domain.Where(this.CompileExpression()), this.SortCriteria);

        public DataStore<T>.IQuery Where(string field, BinaryOperation op, object value) =>
            op.ApplyTo(this, field, value);
        public DataStore<T>.IQuery Where(DataStore<T>.IQuery other) => this.Connector.Apply(this, other);
        public DataStore<T>.IQuery Negate() {
            this.BuiltExpression = Expression.Not(this.BuiltExpression);
            return this;
        }

        private Func<T, bool> CompileExpression() =>
            Expression.Lambda<Func<T, bool>>(this.BuiltExpression 
                                                ?? Expression.Constant(this.Connector.DefaultValue),
                                                DataStore<T>.Param)
                        .Compile();

        public ListQuery(IEnumerable<T> domain, Connector connector) {
            this.Domain = domain;
            this.Connector = connector;
        }

    }
}
