using System.ComponentModel;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Databases {
    public abstract class Connector {

        public enum Connective {
            [Description("All")]
            ALL,
            [Description("Any")]
            ANY
            // TODO: Investigate AtMost and AtLeast
            // BODY: Those seem really hard to implement in most databases.
        }

        // TODO: Find the best implementation of Connector.Apply
        public abstract DataStore<T>.IQuery Apply<T>(DataStore<T>.IQuery left, DataStore<T>.IQuery right)
                where T : Model;

        public bool DefaultValue { get; }

    }
}
