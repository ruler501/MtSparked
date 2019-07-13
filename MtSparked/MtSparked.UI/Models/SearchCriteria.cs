using System;
using System.Collections.Generic;
using System.Text;

namespace MtSparked.Models
{
    public class SearchCriteria : Model
    {
        public static readonly List<string> StringOperations = new List<string>()
        {
            "Contains", "Like", "Starts With", "Ends With", "Equals", "Exists"
        };
        public static readonly List<string> NumberOperations = new List<string>()
        {
            "Equals", "Less Than", "Greater Than"
        };
        public static readonly List<string> NullableNumberOperations = new List<string>()
        {
            "Equals", "Less Than", "Greater Than", "Exists"
        };
        public static readonly List<string> ColorOperations = new List<string>()
        {
            "Contains", "Like", "Equals"
        };

        public string Field { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }
        public bool Set { get; set; }
        public string Color { get; set; }

        private List<string> operations = StringOperations;
        public List<string> Operations { get => operations; set => SetProperty(ref operations, value); }

        private List<string> colors = new List<string>
        {
            "White", "Blue", "Black", "Red", "Green"
        };
        public List<string> Colors { get => colors; set => SetProperty(ref colors, value); }

        public SearchCriteria()
        {
            this.Field = "Name";
            this.Operation = "Contains";
            this.Color = "White";
        }
    }
}
