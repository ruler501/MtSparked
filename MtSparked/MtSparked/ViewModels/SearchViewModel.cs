using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using MtSparked.Models;
using MtSparked.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MtSparked.Services;
using System.Reflection;

namespace MtSparked.ViewModels
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
                    string field = criteria.Field.Replace(" ", "");
                    PropertyInfo property = typeof(Card).GetProperty(field);
                    if (property.PropertyType == typeof(bool))
                    {
                        query.Where(criteria.Field, criteria.Set);
                    }
                    else if (criteria.Operation == "Exists")
                    {
                        query.Where(criteria.Field, criteria.Operation, criteria.Set.ToString());
                    }
                    else if (criteria.Operation == "Contains" && property.PropertyType == typeof(string) && field.Contains("Color"))
                    {
                        query.Where(criteria.Field, criteria.Operation, criteria.Color);
                    }
                    else
                    {
                        query.Where(criteria.Field, criteria.Operation, criteria.Value);
                    }
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