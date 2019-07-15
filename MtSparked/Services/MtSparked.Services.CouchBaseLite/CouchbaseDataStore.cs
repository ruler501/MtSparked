using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQuery<T> : DataStore<T>.IQuery where T : Model {

        public Connector Connector { get; }

        public DataStore<T>.IQuery Negate() => throw new System.NotImplementedException();
        public DataStore<T> ToDataStore() => throw new System.NotImplementedException();
        public DataStore<T>.IQuery Where(string field, BinaryOperation op, object value) => throw new System.NotImplementedException();
        public DataStore<T>.IQuery Where(DataStore<T>.IQuery other) => throw new System.NotImplementedException();

    }
}
