using System.ComponentModel;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Databases {
    public abstract class BinaryOperation {

        public enum BinaryOperator {
            [Description("Equals")]
            EQUALS,
            [Description("Contains")]
            CONTAINS,
            [Description("Like")]
            LIKE,
            [Description("Starts With")]
            STARTS_WITH,
            [Description("Ends With")]
            ENDS_WITH,
            [Description("Matches Regex")]
            MATCHES_REGEX,
            [Description("Contains Regex")]
            CONTAINS_REGEX,
            [Description("Less Than")]
            LESS_THAN,
            [Description("Greater Than")]
            GREATER_THAN,
            [Description("Less Than or Equal to")]
            LESS_THAN_OR_EQUAL,
            [Description("Greater Than or Equal to")]
            GREATER_THAN_OR_EQUAL,
            [Description("Exists")]
            EXISTS
        }

        public BinaryOperator Op { get; }

        public abstract DataStore<T>.IQuery ApplyTo<T>(DataStore<T>.IQuery query,
                                                       string field,
                                                       object value) where T : Model;

    }
}
