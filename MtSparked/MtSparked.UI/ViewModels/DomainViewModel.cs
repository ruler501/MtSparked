﻿using System;
using System.Collections.ObjectModel;

using MtSparked.UI.Models;
using MtSparked.Interop.Models;
using MtSparked.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace MtSparked.UI.ViewModels {
    public class DomainViewModel : Model {

        public ObservableCollection<object> Items { get; private set; } = new ObservableCollection<object>();
        // TODO: Make Enum.
        public string Connector { get; private set; } = "All";
        public bool Negated { get; private set; }

        public DomainCriteria AddCriteria() {
            DomainCriteria criteria = new DomainCriteria();
            this.Items.Add(criteria);
            return criteria;
        }

        public DomainViewModel AddGroup() {
            DomainViewModel model = new DomainViewModel();
            _ = model.AddCriteria();

            this.Items.Add(model);

            return model;
        }

        public static IEnumerable<Card> universe = null;
        public static IEnumerable<Card> Universe => universe ?? (universe = CardDataStore.realm.All<Card>().ToList());

        public IEnumerable<Card> CreateDomain() {
            IEnumerable<Card> result = null;
            foreach(object item in this.Items) {
                IEnumerable<Card> otherSet = null;
                if(item is DomainCriteria criteria) {
                    if(criteria.Field == DomainCriteria.ALL_CARDS) {
                        otherSet = Universe;
                    } else if(criteria.Field == DomainCriteria.ACTIVE_DECK) {
                        otherSet = ConfigurationManager.ActiveDeck.Cards.Select(bi => bi.Card);
                    } else if(criteria.Field is null) {
                        continue;
                    } else {
                        otherSet = ConfigurationManager.ActiveDeck.Boards[criteria.Field].Values.Select(bi => bi.Card);
                    }

                    if (!criteria.Set) {
                        otherSet = Universe.Except(otherSet, new CardEqualityComparer());
                    }
                } else if(item is DomainViewModel model) {
                    otherSet = model.CreateDomain();
                } else {
                    throw new NotImplementedException();
                }

                if(result is null) {
                    result = otherSet;
                } else if(otherSet is null) {
                    continue;
                } else if(this.Connector == "All") {
                    result = result.Intersect(otherSet, new CardEqualityComparer());
                } else if (this.Connector == "Any") {
                    result = result.Union(otherSet, new CardEqualityComparer());
                } else {
                    throw new NotImplementedException();
                }
            }

            if (this.Negated && !(result is null)) {
                result = Universe.Except(result, new CardEqualityComparer());
            }

            return result;
        }

    }
}