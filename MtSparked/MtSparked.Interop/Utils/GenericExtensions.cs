using System.Collections.Generic;

namespace MtSparked.Interop.Utils {
    public static class GenericExtensions {

        public static IDictionary<U, V> Update<U, V>(this IDictionary<U, V> self, IDictionary<U, V> other) {
            foreach (KeyValuePair<U, V> pair in other) {
                self[pair.Key] = pair.Value;
            }
            return self;
        }

        public static IDictionary<U, V> CombineDicts<U, V>(IDictionary<U, V> d1, IDictionary<U, V> d2)
            => new Dictionary<U, V>(d1).Update(d2);
    }
}
