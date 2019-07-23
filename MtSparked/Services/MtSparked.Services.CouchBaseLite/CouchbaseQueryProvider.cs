using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Couchbase.Lite;
using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;
using MtSparked.Interop.Services;
using Remotion.Linq;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQueryProvider<T> : QueryProviderBase, IQueryProvider<T> where T : Model {

        public CouchbaseQueryProvider(Database database)
                : base(Remotion.Linq.Parsing.Structure.QueryParser.CreateDefault(),
                       new CouchbaseQueryExecutor<T>(database)) {
            this.DefaultSortCriteria = new SortCriteria<T>();
            this.DefaultConnector = Connector.Connective.ALL;
        }

        public SortCriteria<T> DefaultSortCriteria { get; }
        public Connector DefaultConnector { get; }

        public DataStore<T>.IQuery All(Connector connector = null, SortCriteria<T> sortCriteria = null)
            => new CouchbaseQuery<T>(connector ?? this.DefaultConnector, sortCriteria ?? this.DefaultSortCriteria, this);
        public override IQueryable<T1> CreateQuery<T1>(Expression expression) {
            if(typeof(T1) != typeof(T)) {
                throw new ArgumentException("T1 must be the same as T", nameof(T1));
            }
            return (IQueryable<T1>)new CouchbaseQuery<T>(this.DefaultConnector, this.DefaultSortCriteria, this, expression);
        }
        public DataStore<T>.IQuery FromEnumerable(IEnumerable<T> enumerable, Connector connector = null)
            => new ListQuery<T>(enumerable, connector ?? this.DefaultConnector);

    }
}
