using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using Gatherer.Models;
using Gatherer.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gatherer.Services;

namespace Gatherer.ViewModels
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<object> Items { get; set; }
        public string Connector { get; set; }
        public bool Negated { get; set; }

        public SearchViewModel()
        {
            Items = new ObservableCollection<object>();
            this.Connector = "All";
        }

        public SearchCriteria AddCriteria()
        {
            SearchCriteria criteria = new SearchCriteria();
            this.Items.Add(criteria);
            return criteria;
        }

        public SearchViewModel AddGroup()
        {
            SearchViewModel model = new SearchViewModel();
            model.AddCriteria();

            this.Items.Add(model);

            return model;
        }

        public CardDataStore.CardsQuery CreateQuery()
        {
            CardDataStore.CardsQuery query = new CardDataStore.CardsQuery(Connector);
            foreach (object item in Items)
            {
                if (item is SearchCriteria criteria)
                {
                    query.Where(criteria.Field, criteria.Operation, criteria.Value);
                }
                if (item is SearchViewModel model)
                {
                    CardDataStore.CardsQuery query2 = model.CreateQuery();
                    query.Where(query2);
                }
            }
            if (Negated)
            {
                query.Negate();
            }
            return query;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}