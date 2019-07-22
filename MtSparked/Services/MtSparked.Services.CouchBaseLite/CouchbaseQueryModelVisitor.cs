using System;
using System.Linq.Expressions;
using Couchbase.Lite.Query;
using MtSparked.Interop.Models;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQueryModelVisitor<T> : QueryModelVisitorBase where T : Model {

        public IExpression WhereExpression { get; private set; }
        public IExpression SelectExpression { get; private set; }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index) => throw new System.NotImplementedException();
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index) => throw new System.NotImplementedException();
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause) => throw new System.NotImplementedException();

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel) {
#pragma warning disable IDE0022 // Use expression body for methods
            base.VisitMainFromClause(fromClause, queryModel);
#pragma warning restore IDE0022 // Use expression body for methods
        }

        public override void VisitOrdering(Remotion.Linq.Clauses.Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index) {
            // TODO: Convert to additions to a Couchbase Lite query?
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index) => throw new System.NotImplementedException();
        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel) {
#pragma warning disable IDE0022 // Use expression body for methods
            base.VisitSelectClause(selectClause, queryModel);
#pragma warning restore IDE0022 // Use expression body for methods
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index) {
            // TODO: Translate into Couchbase.Lite.**.Expression
            Expression<Func<T, bool>> lamdba = whereClause.Predicate as Expression<Func<T, bool>>;
            switch(whereClause) {
            case Expression<Func<T, bool>> lambda:
                return;
            }
        }

    }
}
