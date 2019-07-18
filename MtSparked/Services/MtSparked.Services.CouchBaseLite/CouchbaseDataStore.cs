using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQuery<T> : DataStore<T>.IQuery where T : Model {

        public CouchbaseQuery(Connector connector,
                              Expression expression,
                              SortCriteria<T> sortCriteria,
                              CouchbaseQueryProvider<T> queryProvider) {
            this.Connector = connector;
            this.Expression = expression;
            this.SortCriteria = sortCriteria;
            this.Provider = queryProvider;
        }

        public Connector Connector { get; }
        public Type ElementType => typeof(T);
        public Expression Expression { get; }
        public SortCriteria<T> SortCriteria { get; }

        public IQueryProvider Provider { get; }

        public DataStore<T> ToDataStore() => throw new System.NotImplementedException();

        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
