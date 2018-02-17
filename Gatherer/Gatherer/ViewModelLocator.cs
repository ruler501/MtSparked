using Gatherer.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherer
{
    public static class ViewModelLocator
    {
        static CardsViewModel cardsVM;

        public static CardsViewModel CardsViewModel =>
            cardsVM ?? (cardsVM = new CardsViewModel());
    }
}
