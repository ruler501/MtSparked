using System.Linq;
using MtSparked.Interop.Models;

namespace MtSparked.UI.Views {
    public interface IHasCardQuery {

        IQueryable<Card> GetQuery();

    }
}
