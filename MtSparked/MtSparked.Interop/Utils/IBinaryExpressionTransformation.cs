using System;
using System.Linq.Expressions;

namespace MtSparked.Interop.Utils {
    public interface IBinaryExpressionTransformation {

        Func<Expression, Expression, Expression> Apply { get; }

    }
}
