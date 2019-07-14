using MtSparked.Core.Services;

namespace MtSparked.UI.Views {
    public interface IQueryable {

        CardDataStore.CardsQuery GetQuery();

    }
}
