using System;
using System.ComponentModel;
using System.Linq.Expressions;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Databases {
    public sealed class Connector
            : EnumSingleton<Connector, Connector.Connective>,
              IBinaryExpressionTransformation {

        public enum Connective {
            [Description("All")]
            ALL,
            [Description("Any")]
            ANY
            // TODO: Investigate AtMost and AtLeast
            // BODY: Those seem really hard to implement in most databases.
        }

        static Connector() {
            _ = new Connector(Connective.ALL,
                              defaultValue: true,
                              apply: (l, r) => Expression.And(l, r));
            _ = new Connector(Connective.ANY,
                              defaultValue: false,
                              apply: (l, r) => Expression.Or(l, r));
            EnumSingleton<Connector, Connective>.Close();
        }

        private Connector(Connective value,
                          bool defaultValue,
                          Func<Expression, Expression, Expression> apply)
                : base(value) {
            this.DefaultValue = defaultValue;
            this.Apply = apply;
        }

        public static implicit operator Connector(Connective connective)
            => EnumSingleton<Connector, Connective>.GetInstance(connective);

        public Func<Expression, Expression, Expression> Apply { get; }

        public bool DefaultValue { get; }

    }
}
