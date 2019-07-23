using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Couchbase.Lite.Query;
using MtSparked.Interop.Models;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQueryModelVisitor<T> : QueryModelVisitorBase where T : Model {

        public IExpression WhereExpression { get; private set; } = null;
        public IExpression SelectExpression { get; private set; } = null;
        public IList<IExpression> OrderingExpressions { get; } = new List<IExpression>();

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index) => throw new System.NotImplementedException();
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index) => throw new System.NotImplementedException();
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause) => throw new System.NotImplementedException();

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel) {
#pragma warning disable IDE0022 // Use expression body for methods
            base.VisitMainFromClause(fromClause, queryModel);
#pragma warning restore IDE0022 // Use expression body for methods
        }

        public override void VisitOrdering(Remotion.Linq.Clauses.Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index) {
#pragma warning disable IDE0022 // Use expression body for methods
            this.OrderingExpressions.Add(CouchbaseExpressionUtils.FromLinq(ordering.Expression));
#pragma warning restore IDE0022 // Use expression body for methods
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index) => throw new System.NotImplementedException();
        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel) {
#pragma warning disable IDE0022 // Use expression body for methods
            this.SelectExpression = CouchbaseExpressionUtils.FromLinq(selectClause.Selector);
#pragma warning restore IDE0022 // Use expression body for methods
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index) {
#pragma warning disable IDE0022 // Use expression body for methods
            this.WhereExpression = CouchbaseExpressionUtils.FromLinq(whereClause.Predicate);
#pragma warning restore IDE0022 // Use expression body for methods
        }

    }
}
