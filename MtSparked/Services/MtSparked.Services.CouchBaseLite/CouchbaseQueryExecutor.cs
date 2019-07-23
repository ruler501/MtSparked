using Remotion.Linq;

using MtSparked.Interop.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using Couchbase.Lite;
using Couchbase.Lite.Query;

namespace MtSparked.Services.CouchBaseLite {
    // TODO: Implement
    public class CouchbaseQueryExecutor<T> : IQueryExecutor where T : Model {

        private Database Database { get; }

        public CouchbaseQueryExecutor(Database database) {
            this.Database = database;
        }

        public IEnumerable<T> ExecuteCollection(QueryModel queryModel) {
            CouchbaseQueryModelVisitor<T> visitor = new CouchbaseQueryModelVisitor<T>();
            visitor.VisitQueryModel(queryModel);
            // TODO: Custom DataSource and Select
            IWhereRouter whereBuilder = QueryBuilder.Select(SelectResult.All()).From(DataSource.Database(this.Database));
            IOrderByRouter orderByBuilder = (IOrderByRouter)whereBuilder;
            if (!(visitor.WhereExpression is null)) {
                orderByBuilder = whereBuilder.Where(visitor.WhereExpression);
            }
            IQuery builder = (IQuery)orderByBuilder;
            if (visitor.OrderingExpressions.Count > 0) {
                builder = orderByBuilder.OrderBy(visitor.OrderingExpressions.Select(oe => Ordering.Expression(oe)).ToArray());
            }
            IResultSet results = builder.Execute();
            return results.Select(CouchbaseDeserializer.FromResult<T>);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public T ExecuteScalar(QueryModel queryModel) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter

#pragma warning disable IDE0060 // Remove unused parameter
        public T ExecuteSingle(QueryModel queryModel, bool returnDefaultWhenEmpty = false) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter

        public IEnumerable<T1> ExecuteCollection<T1>(QueryModel queryModel) {
            if (!typeof(T1).IsAssignableFrom(typeof(T))) {
                throw new ArgumentException("Can only query for things T can be assigned to.", nameof(T1));
            }
            return this.ExecuteCollection(queryModel).Cast<T1>();
        }

        public T1 ExecuteScalar<T1>(QueryModel queryModel) {
            if (!typeof(T1).IsAssignableFrom(typeof(T))) {
                throw new ArgumentException("Can only query for things T can be assigned to.", nameof(T1));
            }
            throw new NotImplementedException();
            // TODO: Figure out the correct way to write this
            // return ((object)this.ExecuteSingle(queryModel, returnDefaultWhenEmpty)) as T1;
        }

        public T1 ExecuteSingle<T1>(QueryModel queryModel, bool returnDefaultWhenEmpty) {
            if (!typeof(T1).IsAssignableFrom(typeof(T))) {
                throw new ArgumentException("Can only query for things T can be assigned to.", nameof(T1));
            }
            throw new NotImplementedException();
            // TODO: Figure out the correct way to write this
            // return ((object)this.ExecuteSingle(queryModel, returnDefaultWhenEmpty)) as T1;
        }

    }
}