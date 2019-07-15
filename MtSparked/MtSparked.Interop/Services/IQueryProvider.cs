using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Services {
    public interface IQueryProvider<T> where T : Model {

        SortCriteria<T> DefaultSortCriteria { get; set; }
        Connector DefaultConnector { get; set; }

        DataStore<T>.IQuery Where(string field, BinaryOperation op, object value);

        DataStore<T> All();

    }
}
