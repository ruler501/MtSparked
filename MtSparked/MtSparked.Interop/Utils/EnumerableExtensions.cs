using System;
using System.Collections.Generic;
using System.Linq;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Utils {
    public static class EnumerableExtensions {

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
                                                                     Func<TSource, TKey> keySelector) {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source) {
                if (seenKeys.Add(keySelector(element))) {
                    yield return element;
                }
            }
        }

        public static IEnumerable<EnhancedGrouping<TSource>> EnhancedGroupBy<TSource>(this IEnumerable<TSource> self,
                                                                         Func<TSource, string> labeler) =>
            self.GroupBy(labeler).Select(grouping => new EnhancedGrouping<TSource>(grouping));

    }
}
