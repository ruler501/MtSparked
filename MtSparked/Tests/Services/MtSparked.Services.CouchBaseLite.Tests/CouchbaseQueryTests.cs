using Xunit;

using Couchbase.Lite;

namespace MtSparked.Services.CouchBaseLite.Tests {
    public class CouchbaseQueryTests {

        static CouchbaseQueryTests() {
            Couchbase.Lite.Support.NetDesktop.Activate();
        }

        [Fact(Skip = "Not Implemented Yet.")]
        public void TestGetEnumerator() {
            Database database = new Database("Test");
        }
    }
}
