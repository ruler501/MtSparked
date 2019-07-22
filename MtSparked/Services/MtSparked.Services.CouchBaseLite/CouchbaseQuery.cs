using System;
using System.Linq.Expressions;
using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQuery<T> : QueryableBase<T>, DataStore<T>.IQuery where T : Model {

        public CouchbaseQuery(Connector connector,
                              SortCriteria<T> sortCriteria,
                              CouchbaseQueryExecutor<T> executor)
                : base(QueryParser.CreateDefault(), executor) {
            this.Connector = connector;
            this.SortCriteria = sortCriteria;
        }

        public CouchbaseQuery(IQueryParser queryParser, IQueryExecutor executor) : base(queryParser, executor) {
        }

        public Connector Connector { get; }
        public Expression<Func<T, bool>> CompiledExpression => Expression.Lambda<Func<T, bool>>(this.Expression, DataStore<T>.Param);
        public SortCriteria<T> SortCriteria { get; }

        public DataStore<T> ToDataStore() => throw new NotImplementedException();
    }
}
