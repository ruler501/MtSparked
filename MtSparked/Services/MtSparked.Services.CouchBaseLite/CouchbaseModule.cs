using System.Linq;
using Autofac;
using MtSparked.Interop.Models;
using MtSparked.Interop.Services;

namespace MtSparked.Services.CouchBaseLite {
    public class CouchbaseModule : Module {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
        protected override void Load(ContainerBuilder builder) {
            _ = builder.RegisterGeneric(typeof(CouchbaseQueryProvider<>)).As(typeof(IQueryProvider<>));
        }

    }
}
