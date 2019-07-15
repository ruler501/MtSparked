using MtSparked.Interop.Databases;
using MtSparked.Interop.Models;
using MtSparked.Interop.Services;

namespace MtSparked.UI.Views {
    public interface IHasCardQuery {

        DataStore<Card>.IQuery GetQuery();

    }
}
