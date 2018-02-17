using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherer.Models
{
    public class SearchCriteria
    {
        public string Field { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }

        public SearchCriteria()
        {
            this.Field = "Name";
            this.Operation = "Contains";
        }
    }
}
