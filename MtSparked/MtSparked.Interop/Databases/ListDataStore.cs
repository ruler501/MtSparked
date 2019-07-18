using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Databases {
    public class ListQuery<T> : DataStore<T>.IQuery where T : Model {

        private Expression BuiltExpression { get; set; }

        public Connector Connector { get; }

        public SortCriteria<T> SortCriteria { get; }

        public IEnumerable<T> Domain { get; }
        public Type ElementType => typeof(T);

        public Expression Expression => this.BuiltExpression;
        // TODO: Needs a Provider.
        public IQueryProvider Provider { get; } = null;

        public DataStore<T> ToDataStore() =>
            new DataStore<T>(this.Domain.Where(this.CompileExpression()), this.SortCriteria);

        private Func<T, bool> CompileExpression() =>
            Expression.Lambda<Func<T, bool>>(this.BuiltExpression 
                                                ?? Expression.Constant(this.Connector.DefaultValue),
                                                DataStore<T>.Param)
                        .Compile();
        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        public ListQuery(IEnumerable<T> domain, Connector connector) {
            this.Domain = domain;
            this.Connector = connector;
            this.BuiltExpression = Expression.Constant(connector.DefaultValue);
        }

    }
}
