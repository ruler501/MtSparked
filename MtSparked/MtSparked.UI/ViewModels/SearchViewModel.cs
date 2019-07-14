using System.Collections.ObjectModel;

using MtSparked.Interop.Models;
using MtSparked.Core.Services;
using System.Reflection;
using System.Collections.Generic;
using MtSparked.UI.Models;

namespace MtSparked.UI.ViewModels {
    public class SearchViewModel : Model {

        // TODO: Better restriction on typing for items.
        public ObservableCollection<object> Items { get; private set; } = new ObservableCollection<object>();
        public string Connector { get; private set; } = "All";
        public bool Negated { get; private set; } = false;

        public SearchCriteria AddCriteria() {
            SearchCriteria criteria = new SearchCriteria();
            this.Items.Add(criteria);
            return criteria;
        }

        public SearchViewModel AddGroup() {
            SearchViewModel model = new SearchViewModel();
            _ = model.AddCriteria();

            this.Items.Add(model);

            return model;
        }

        public CardDataStore.CardsQuery CreateQuery(IEnumerable<Card> domain = null) {
            CardDataStore.CardsQuery query = new CardDataStore.CardsQuery(this.Connector, domain);
            foreach (object item in this.Items) {
                if (item is SearchCriteria criteria) {
                    string field = criteria.Field.Replace(" ", "");
                    PropertyInfo property = typeof(Card).GetProperty(field);
                    if (property.PropertyType == typeof(bool)) {
                        query = query.Where(criteria.Field, criteria.Set);
                    } else if (criteria.Operation == "Exists") {
                        query = query.Where(criteria.Field, criteria.Operation, criteria.Set.ToString());
                    } else if (criteria.Operation == "Contains" &&
                            property.PropertyType == typeof(string) && field.Contains("Color")) {
                        query = query.Where(criteria.Field, criteria.Operation, criteria.Color);
                    } else {
                        query = query.Where(criteria.Field, criteria.Operation, criteria.Value);
                    }
                } else if (item is SearchViewModel model) {
                    CardDataStore.CardsQuery query2 = model.CreateQuery(domain);
                    _ = query.Where(query2);
                }
            }
            if (this.Negated) {
                query = query.Negate();
            }
            return query;
        }

    }
}