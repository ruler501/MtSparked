using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using MtSparked.Interop.Models;
using MtSparked.Interop.Utils;
using Realms;

namespace MtSparked.Core.Services {
    public class CardDataStore {

        public List<EnhancedGrouping<Card>> Items { get; set; }
        private Expression<Func<Card, bool>> Query { get; }
        private IEnumerable<Card> Domain { get; }

        public static Realm realm = null;

        static CardDataStore() {
            RealmConfiguration config = new RealmConfiguration("cards.db");
            realm = Realm.GetInstance(config);
        }

        protected CardDataStore(Expression<Func<Card, bool>> query, IEnumerable<Card> domain = null) {
            this.Query = query;
            this.Domain = domain;

            this.LoadCards();
        }

        public void LoadCards() {
            IEnumerable<Card> mockItems;
            if (this.Domain is null) {
                mockItems = realm.All<Card>().Where(this.Query);
            } else {
                mockItems = this.Domain.Where(this.Query.Compile());
            }

            if (ConfigurationManager.ShowUnique) {
                mockItems = mockItems.DistinctBy(c => c.Name);
            }

            IEnumerable<IGrouping<string, Card>> grouping = null;
            Func<string, string> labelFunc = null;
            // TODO #64: Make SortCriteria Simpler to Work With
            // BODY: Come up with a more compact way to specify this so we don't have this absurd if cascade.
            if (ConfigurationManager.SortCriteria == "Cmc") {
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.Cmc.ToString());
                }
            } else if (ConfigurationManager.SortCriteria == "Name") {
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.Name);
                } if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => new string(c.Name.FirstOrDefault(), 1));
                }
            } else if (ConfigurationManager.SortCriteria == "Set Name") {
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(c => c.SetName).ThenBy(c => c.Cmc).ThenBy(c => c.Name); ;
                } else {
                    mockItems = mockItems.OrderBy(c => c.SetName).ThenBy(c => c.Cmc).ThenBy(c => c.Name); ;
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.SetName);
                }
            } else if (ConfigurationManager.SortCriteria == "Colors") {
                if (ConfigurationManager.DescendingSort) {
                    // TODO #65: Custom Class for Dealing with Color
                    mockItems = mockItems.OrderByDescending(c => c.Colors.Length).ThenByDescending(c => c.Colors)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.Colors.Length).ThenBy(c => c.Colors)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.Colors);
                }
            } else if (ConfigurationManager.SortCriteria == "Color Identity") {
                if (ConfigurationManager.DescendingSort) {
                    // TODO #65: Custom Class for Dealing with Color
                    mockItems = mockItems.OrderByDescending(c => c.ColorIdentity.Length).ThenByDescending(c => c.ColorIdentity)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.ColorIdentity.Length).ThenBy(c => c.ColorIdentity)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.ColorIdentity);
                }
            } else if (ConfigurationManager.SortCriteria == "Power") {
                int powerToInt(Card c) {
                    if (Int32.TryParse(c.Power ?? "0", out int power)) {
                        return power;
                    } else {
                        return 0;
                    }
                }
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(powerToInt).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(powerToInt).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.Power ?? "");
                }
            } else if (ConfigurationManager.SortCriteria == "Toughness") {
                int toughnessToInt(Card c) {
                    if (Int32.TryParse(c.Toughness ?? "0", out int toughness)) {
                        return toughness;
                    }
                    else {
                        return 0;
                    }
                }
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(toughnessToInt).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(toughnessToInt).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.Toughness ?? "");
                }
            } else if (ConfigurationManager.SortCriteria == "Market Price") {
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(c => c.MarketPrice ?? Double.PositiveInfinity).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.MarketPrice ?? Double.PositiveInfinity).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.MarketPrice?.ToString() ?? "N/A");
                }
            } else if (ConfigurationManager.SortCriteria == "Rarity") {
                if (ConfigurationManager.DescendingSort) {
                    // TODO #66: Make Rarity an Enum
                    mockItems = mockItems.OrderByDescending(c => c.Rarity).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.Rarity).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.Rarity);
                }
            } else if (ConfigurationManager.SortCriteria == "Life") {
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(c => c.Life ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.Life ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.Life?.ToString() ?? "N/A");
                }
            } else if (ConfigurationManager.SortCriteria == "Hand") {
                if (ConfigurationManager.DescendingSort) {
                    mockItems = mockItems.OrderByDescending(c => c.Hand ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } else {
                    mockItems = mockItems.OrderBy(c => c.Hand ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                } if (ConfigurationManager.CountByGroup) {
                    grouping = mockItems.GroupBy(c => c.Hand?.ToString() ?? "N/A");
                }
            } else {
                throw new NotImplementedException();
            }

            if (!ConfigurationManager.CountByGroup) {
                grouping = mockItems.GroupBy(c => "Total");
            }

            this.Items = grouping.Select(g => new EnhancedGrouping<Card>(g, labelFunc)).ToList();
        }
        
        public class CardsQuery {

            private Expression BuiltExpression { get; set; } = null;
            private static ParameterExpression Param { get; } = Expression.Parameter(typeof(Card), "card");
            // TODO #67: Make Connector a Custom Class or Enum
            private string Connector { get; }

            public IEnumerable<Card> Domain { get; private set; }

            public CardsQuery(string connector, IEnumerable<Card> domain = null) {
                this.Connector = connector;
                this.Domain = domain;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
            public static CardsQuery FromString(string query) {
                // TODO #68: Implement Language for CardsQueries
                return null;
            }

            public CardsQuery Where(string field, bool set) {
                field = field.Replace(" ", "");
                Expression property = Expression.Property(CardsQuery.Param, field);
                Expression constant = Expression.Constant(set, typeof(bool));
                Expression fullCombine = Expression.Equal(property, constant);

                if (this.BuiltExpression is null) {
                    this.BuiltExpression = fullCombine;
                } else if (this.Connector == "All") {
                    this.BuiltExpression = Expression.AndAlso(this.BuiltExpression, fullCombine);
                } else if (this.Connector == "Any") {
                    this.BuiltExpression = Expression.OrElse(this.BuiltExpression, fullCombine);
                } else {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Where(string field, string op, string value)
            {
                if(value is null || String.IsNullOrWhiteSpace(value)) {
                    return this;
                }
                field = field.Replace(" ", "");
                value = value.Trim();
                if (field.Contains("Color")) {
                    // TODO #65: Custom Class for Dealing with Color 
                    if (value.Equals("White", StringComparison.OrdinalIgnoreCase)) {
                        value = "W";
                    } else if (value.Equals("Blue", StringComparison.OrdinalIgnoreCase)) {
                        value = "U";
                    } else if (value.Equals("Black", StringComparison.OrdinalIgnoreCase)) {
                        value = "B";
                    } else if (value.Equals("Red", StringComparison.OrdinalIgnoreCase)) {
                        value = "R";
                    } else if (value.Equals("Green", StringComparison.OrdinalIgnoreCase)) {
                        value = "G";
                    } else {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                }

                Expression property = Expression.Property(Param, field);
                Expression constant;
                PropertyInfo propertyInfo = typeof(Card).GetProperty(field);
                if(propertyInfo.PropertyType == typeof(bool) || op == "Exists") {
                    bool val = Boolean.Parse(value);
                    constant = Expression.Constant(val, typeof(bool));
                } else if(propertyInfo.PropertyType == typeof(int?)) {
                    constant = Expression.Constant(Int32.Parse(value), typeof(int?));
                } else if (propertyInfo.PropertyType == typeof(int)) {
                    constant = Expression.Constant(Int32.Parse(value), typeof(int));
                } else if(propertyInfo.PropertyType == typeof(double?)) {
                    constant = Expression.Constant(Double.Parse(value), typeof(double?));
                } else if (propertyInfo.PropertyType == typeof(double)) {
                    constant = Expression.Constant(Double.Parse(value), typeof(double));
                } else {
                    throw new ArgumentOutOfRangeException(nameof(field));
                }
                Expression fullCombine = null;
                // TODO #70: CardsQuery.Op needs to be a Custom Class
                if (op == "Equals") {
                    // TODO #71: Configuration Setting or Separate Operator to do Case Sensitive Matching
                    if (propertyInfo.PropertyType == typeof(string)) {
                        Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        fullCombine = Expression.Call(property, "Equals", Type.EmptyTypes, new[] { constant, caseInsensitive });
                    } else {
                        fullCombine = Expression.Equal(property, constant);
                    }
                } else if(op == "Contains") {
                    if (propertyInfo.PropertyType == typeof(string)) {
                        Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        fullCombine = Expression.Call(typeof(StringExtensions), "Contains", Type.EmptyTypes, property, constant, caseInsensitive);
                    }
                } else if(op == "Like") {
                    fullCombine = Expression.Call(typeof(StringExtensions), "Like", Type.EmptyTypes, property, constant);
                } else if(op == "Starts With") {
                    Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                    fullCombine = Expression.Call(property, "StartsWith", Type.EmptyTypes, constant, caseInsensitive);
                } else if (op == "Ends With") {
                    Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                    fullCombine = Expression.Call(property, "EndsWith", Type.EmptyTypes, constant, caseInsensitive);
                } else if(op == "Less Than") {
                    fullCombine = Expression.LessThan(property, constant);
                } else if(op == "Greater Than") {
                    fullCombine = Expression.GreaterThan(property, constant);
                } else if(op == "Exists") {
                    Expression nullConstant = Expression.Constant(null);
                    bool exists = Boolean.Parse(value);
                    if (exists) {
                        fullCombine = Expression.NotEqual(property, nullConstant);
                    } else {
                        fullCombine = Expression.Equal(property, nullConstant);
                    }
                } else {
                    throw new NotImplementedException();
                }

                if (this.BuiltExpression is null) {
                    this.BuiltExpression = fullCombine;
                } else if (this.Connector == "All") {
                    this.BuiltExpression = Expression.AndAlso(this.BuiltExpression, fullCombine);
                } else if (this.Connector == "Any") {
                    this.BuiltExpression = Expression.OrElse(this.BuiltExpression, fullCombine);
                } else {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Where(CardsQuery other)
            {
                Expression otherExpression = other?.BuiltExpression;
                if(otherExpression is null) {
                    throw new ArgumentNullException(nameof(other));
                }
                if(this.BuiltExpression is null) {
                    this.BuiltExpression = other.BuiltExpression;
                } else if (this.Connector == "All") {
                    this.BuiltExpression = Expression.AndAlso(this.BuiltExpression, otherExpression);
                } else if (this.Connector == "Any") {
                    this.BuiltExpression = Expression.OrElse(this.BuiltExpression, otherExpression);
                } else {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Negate()
            {
                if(this.BuiltExpression is null) {
                    throw new Exception("Cannot negate an empty query");
                } else {
                    this.BuiltExpression = Expression.Not(this.BuiltExpression);
                }

                return this;
            }

            private Expression<Func<Card, bool>> BuildExpression() => Expression.Lambda<Func<Card, bool>>(this.BuiltExpression ?? Expression.Constant(true), CardsQuery.Param);

            public CardDataStore ToDataStore() => new CardDataStore(this.BuildExpression(), this.Domain);

            public IList<Card> ToList() {
                if(this.Domain is null) {
                    return realm.All<Card>().Where(this.BuildExpression()).ToList();
                } else {
                    return this.Domain.Where(this.BuildExpression().Compile()).ToList();
                }
            }

        }

        public static CardsQuery Where(string field, string op, string value, string connector = "All") => new CardsQuery(connector).Where(field, op, value);
        public static CardsQuery Where(string field, bool set, string connector = "All") => new CardsQuery(connector).Where(field, set);

        public static Card ById(string id) => realm.Find<Card>(id);
        public static Card ByMvid(string mvid) => realm.All<Card>().Where(c => c.MultiverseId == mvid).FirstOrDefault();
    }
}