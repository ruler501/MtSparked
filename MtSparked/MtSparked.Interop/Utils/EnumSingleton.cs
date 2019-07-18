using System;
using System.Collections.Generic;

namespace MtSparked.Interop.Utils {
    public abstract class EnumSingleton<T, Values> where T : EnumSingleton<T, Values> 
                                                   where Values : Enum {

        private static IDictionary<Values, T> InstanceStore { get; } = new Dictionary<Values, T>();

        private static bool Closed { get; set; } = false;
        public static implicit operator EnumSingleton<T, Values>(Values member)
            => EnumSingleton<T, Values>.InstanceStore[member];

        public static T GetInstance(Values value)
            => EnumSingleton<T, Values>.InstanceStore[value];

        public Values Value { get; }

        protected EnumSingleton(Values value) {
            if (!(this is T tified)) {
                throw new Exception("T must be the only class to inherit directly from EnumSingleton<T, Values>.");
            } else if (EnumSingleton<T, Values>.Closed
                        || EnumSingleton<T, Values>.InstanceStore.ContainsKey(value)
                        || Enum.IsDefined(typeof(Values), value)) {
                throw new ArgumentException("Value must be unique", nameof(value));
            }
            this.Value = value;
            EnumSingleton<T, Values>.InstanceStore[value] = tified;
        }

        protected static void Close() {
            EnumSingleton<T, Values>.Closed = true;
            if (EnumSingleton<T, Values>.InstanceStore.Keys.Count
                 != Enum.GetValues(typeof(Values)).Length) {
                throw new Exception("Did not cover all cases.");
            }
        }

    }
}
