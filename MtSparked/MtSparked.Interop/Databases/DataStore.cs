using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using MtSparked.Interop.Models;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Databases {
    public abstract class DataStore : Model, IEnumerable {

        public const string ParamName = "model";

        public abstract IEnumerator GetEnumerator();

        public interface IQuery : IOrderedQueryable {

            Connector Connector { get; }

        }

    }

    public class DataStore<T> : Model, IEnumerable<T> where T : Model {

        public DataStore(IEnumerable<T> items, SortCriteria<T> sortCriteria) {
            this.SortCriteria = sortCriteria;
            this.AllItems = items;
        }

        public IEnumerable<T> AllItems { get; }

        private SortCriteria<T> sortCriteria;
        public SortCriteria<T> SortCriteria {
            get { return this.sortCriteria; }
            set {
                _ = this.SetProperty(ref this.sortCriteria, value);
                this.Reload();
            }
        }

        public static ParameterExpression Param { get; } = Expression.Parameter(typeof(T), DataStore.ParamName);

        private ObservableCollection<EnhancedGrouping<T>> items;
        public ObservableCollection<EnhancedGrouping<T>> Items {
            get { return this.items; }
            set { _ = this.SetProperty(ref this.items, value); }
        }

        public void Reload() {
            this.Items = new ObservableCollection<EnhancedGrouping<T>>(
                this.AllItems.EnhancedGroupBy(this.SortCriteria.Grouping.CompiledLambda));
        }

        public IEnumerator<T> GetEnumerator() => this.AllItems.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.AllItems.GetEnumerator();

        public interface IQuery: Databases.DataStore.IQuery, IOrderedQueryable<T> {

            SortCriteria<T> SortCriteria { get; }

            DataStore<T> ToDataStore();

        }

    }
}