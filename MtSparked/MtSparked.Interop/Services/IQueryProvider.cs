using System.Collections.Generic;
using System.Linq;
using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Services {
    public interface IQueryProvider<T> : IQueryProvider where T : Model {

        ISortCriteria DefaultSortCriteria { get; }
        Connector DefaultConnector { get; }

        DataStore<T>.IQuery All(Connector connector = null);
        // Wish we could supply a default here.
        DataStore<T>.IQuery FromEnumerable(IEnumerable<T> enumerable, Connector connector = null);
    }
}
