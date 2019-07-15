using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using MtSparked.Interop.Models;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Databases {
    public class DataStore<T> : Model where T : Model {

        internal DataStore(IEnumerable<T> items, SortCriteria<T> sortCriteria) {
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

        public static ParameterExpression Param { get; } = Expression.Parameter(typeof(T), typeof(T).Name);

        private ObservableCollection<EnhancedGrouping<T>> items;
        public ObservableCollection<EnhancedGrouping<T>> Items {
            get { return this.items; }
            set { _ = this.SetProperty(ref this.items, value); }
        }

        public void Reload() {
            this.Items = new ObservableCollection<EnhancedGrouping<T>>(
                this.AllItems.EnhancedGroupBy(this.SortCriteria.Grouping.CompiledExpressionWithType));
        }

        public interface IQuery {

            Connector Connector { get; }

            IQuery Where(string field, BinaryOperation op, object value);
            IQuery Where(IQuery other);
            IQuery Negate();

            DataStore<T> ToDataStore();

        }

    }
}
