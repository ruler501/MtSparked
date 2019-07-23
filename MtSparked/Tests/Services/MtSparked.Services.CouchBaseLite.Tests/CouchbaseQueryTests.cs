using Xunit;

using Couchbase.Lite;
using MtSparked.Interop.Models;
using System.Linq;
using System.Collections.Generic;

namespace MtSparked.Services.CouchBaseLite.Tests {
    public class CouchbaseQueryTests {

        static CouchbaseQueryTests() {
            Couchbase.Lite.Support.NetDesktop.Activate();
        }

        [Fact]
        public void TestGetEnumerator() {
            Database database = new Database("Test");
            CouchbaseQueryProvider<Card> provider = new CouchbaseQueryProvider<Card>(database);
            IQueryable<Card> query = provider.All().Where(model => model.Name.Contains("ar") && model.Cmc >= 5).OrderBy(model => model.TypeLine);
            IList<Card> cards = query.ToList();
            Assert.NotNull(cards);
        }
    }
}
