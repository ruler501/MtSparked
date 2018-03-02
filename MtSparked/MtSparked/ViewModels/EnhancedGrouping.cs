using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtSparked.ViewModels
{
    public class EnhancedGrouping<T> : List<T>
    {
        public string Key { get; set; }
        public string ShortName { get; set; }

        public EnhancedGrouping(IGrouping<string, T> grouping, Func<string, string> shortName=null)
        : base(grouping)
        {
            this.Key = grouping.Key + ": " + grouping.Count();
            if(shortName is null)
            {
                this.ShortName = grouping.Key;
            }
            else
            {
                this.ShortName = shortName(grouping.Key);
            }
        }
    }
}
