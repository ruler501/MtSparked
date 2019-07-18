using System;
using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;
using MtSparked.Interop.Services;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseQueryProvider<T> : IQueryProvider<T> where T : Model {

        public CouchbaseQueryProvider()
            : base(new SortCriteria<T>(), Connector.Connective.ALL)
        { }

        public override DataStore<T>.IQuery All(Connector connector = null) => throw new NotImplementedException();
        
    }
}
