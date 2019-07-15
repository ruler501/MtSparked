using System;
using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;
using MtSparked.Interop.Services;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQueryProvider<T> : IQueryProvider<T> where T : Model {

        public SortCriteria<T> DefaultSortCriteria { get; set; }
        public Connector DefaultConnector { get; set; }

        public DataStore<T> All() => throw new NotImplementedException();
        public DataStore<T>.IQuery Where(string field, BinaryOperation op, object value) => throw new NotImplementedException();

    }
}
