using MtSparked.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtSparked.Views
{
    public interface IQueryable
    {
        CardDataStore.CardsQuery GetQuery();
    }
}
