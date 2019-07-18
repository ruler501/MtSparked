using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Services {
    public abstract class IQueryProvider<T> : IQueryProvider where T : Model {

        protected IQueryProvider(ISortCriteria defaultSortCriteria,
                                 Connector defaultConnector) {
            this.DefaultSortCriteria = defaultSortCriteria;
            this.DefaultConnector = defaultConnector;
        }

        public ISortCriteria DefaultSortCriteria { get; }
        public Connector DefaultConnector { get; }

        public abstract DataStore<T>.IQuery All(Connector connector = null);
        public DataStore<T>.IQuery FromEnumerable(IEnumerable<T> enumerable, Connector connector = null)
            => new ListQuery<T>(enumerable, connector ?? this.DefaultConnector);
        public IQueryable CreateQuery(Expression expression) => throw new System.NotImplementedException();
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => throw new System.NotImplementedException();

        public object Execute(Expression expression) => throw new System.NotImplementedException();
        public TResult Execute<TResult>(Expression expression) => throw new System.NotImplementedException();
    }
}
