using System.Linq;
using System.Linq.Expressions;
using MtSparked.Interop.Models;

namespace MtSparked.Interop.Databases {
    public static class QueryableExtensions {

        public static IQueryable<T> Connect<T>(this IQueryable<T> self,
                                               Connector connector,
                                               IQueryable<T> right)
            => self.Provider.CreateQuery<T>(connector.Apply(self.Expression, right.Expression));

        public static IQueryable<T> Where<T>(this IQueryable<T> self,
                                             Expression left,
                                             BinaryOperation op,
                                             Expression right)
            => self.Provider.CreateQuery<T>(op.Apply(left, right));

        public static IQueryable<T> Where<T>(this IQueryable<T> self,
                                             string field,
                                             BinaryOperation op,
                                             object value) where T : Model
            => self.Provider.CreateQuery<T>(op.ApplyWith<T>(field, value));

        public static IQueryable<T> Negate<T>(this IQueryable<T> self)
            => self.Provider.CreateQuery<T>(Expression.Negate(self.Expression));

    }
}
