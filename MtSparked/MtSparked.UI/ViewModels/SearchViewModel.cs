using System.Collections.ObjectModel;

using MtSparked.Interop.Models;
using MtSparked.Interop.Services;
using System.Reflection;
using System.Collections.Generic;
using MtSparked.UI.Models;
using MtSparked.Interop.Databases;
using System.Linq;

namespace MtSparked.UI.ViewModels {
    public class SearchViewModel : Model {

        // TODO #76: Make ItemsSources as Type Strict as Possible
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

        public IQueryable<Card> CreateQuery(IEnumerable<Card> domain = null) {
            // TODO: Update to use Connectors correctly.
            IQueryable<Card> query = new ListQuery<Card>(domain, null);
            foreach (object item in this.Items) {
                if (item is SearchCriteria criteria) {
                    string field = criteria.Field.Replace(" ", "");
                    PropertyInfo property = typeof(Card).GetProperty(field);
                    if (property.PropertyType == typeof(bool)) {
                        // query = query.Where(criteria.Field, criteria.Set);
                    } else if (criteria.Operation == "Exists") {
                        // TODO: Update to use BinaryOperations correctly
                        // query = query.Where(criteria.Field, criteria.Operation, criteria.Set.ToString());
                    } else if (criteria.Operation == "Contains" &&
                            property.PropertyType == typeof(string) && field.Contains("Color")) {
                        // query = query.Where(criteria.Field, criteria.Operation, criteria.Color);
                    } else {
                        // query = query.Where(criteria.Field, criteria.Operation, criteria.Value);
                    }
                } else if (item is SearchViewModel model) {
                    IQueryable<Card> query2 = model.CreateQuery(domain);
                    // TODO: Get the extensions working correctly with IQuery
                    // query = query.Connect(query.Connector, query2);
                }
            }
            if (this.Negated) {
                query = query.Negate();
            }
            return query;
        }

    }
}