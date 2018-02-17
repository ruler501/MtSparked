using Gatherer.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherer.Views
{
    public interface IQueryable
    {
        CardDataStore.CardsQuery GetQuery();
    }
}
