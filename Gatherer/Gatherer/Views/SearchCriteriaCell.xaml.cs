using Gatherer.Models;
using Gatherer.Services;
using Gatherer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchCriteriaCell : ContentView, IQueryable
    {
        public SearchCriteriaCell(SearchCriteria criteria)
        {
            InitializeComponent();

            this.BindingContext = criteria;
        }

        public CardDataStore.CardsQuery GetQuery()
        {
            if((BindingContext as SearchCriteria) is null)
            {
                throw new Exception("Invalid Binding Context");
            }

            SearchCriteria criteria = (SearchCriteria)BindingContext;

            return CardDataStore.Where(criteria.Field, criteria.Operation, criteria.Value);
        }
    }
}