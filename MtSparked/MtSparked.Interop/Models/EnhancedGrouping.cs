using System;
using System.Collections.Generic;
using System.Linq;

namespace MtSparked.Interop.Models {
    public class EnhancedGrouping<T> : List<T> {

        public string Key { get; set; }
        public string ShortName { get; set; }

        public EnhancedGrouping(IGrouping<string, T> grouping, Func<string, string> shortName=null)
            : base(grouping)
        {
            this.Key = $"{grouping.Key}: {grouping.Count()}";
            shortName = shortName ?? (x => x);
            this.ShortName = shortName(grouping.Key);
        }

    }
}
