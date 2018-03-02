using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using MtSparked.Models;
using MtSparked.ViewModels;
using Realms;

namespace MtSparked.Services
{
    public class CardDataStore
    {
        public List<EnhancedGrouping<Card>> Items { get; set; }
        Expression<Func<Card, bool>> Query;
        IEnumerable<Card> Domain;

        public static Realm realm = null;

        static CardDataStore()
        {
            RealmConfiguration config = new RealmConfiguration("cards.db");
            realm = Realm.GetInstance(config);
        }

        protected CardDataStore(Expression<Func<Card, bool>> query, IEnumerable<Card> domain = null)
        {
            this.Query = query;
            this.Domain = domain;

            this.LoadCards();
        }

        public void LoadCards()
        {
            IEnumerable<Card> mockItems;
            if (this.Domain is null)
            {
                mockItems = realm.All<Card>().Where(Query);
            }
            else
            {
                mockItems = Domain.Where(Query.Compile());
            }

            if (ConfigurationManager.ShowUnique)
            {
                mockItems = mockItems.DistinctBy(c => c.Name);
            }

            IEnumerable<IGrouping<string, Card>> grouping = null;
            Func<string, string> labelFunc = null;
            if(ConfigurationManager.SortCriteria == "Cmc")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.Cmc.ToString());
                }
            }
            else if (ConfigurationManager.SortCriteria == "Name")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => new string(c.Name.FirstOrDefault(), 1));
                }
            }
            else if (ConfigurationManager.SortCriteria == "Set Name")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.SetName).ThenBy(c => c.Cmc).ThenBy(c => c.Name); ;
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.SetName).ThenBy(c => c.Cmc).ThenBy(c => c.Name); ;
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.SetName);
                }
            }
            else if (ConfigurationManager.SortCriteria == "Colors")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.Colors.Length).ThenByDescending(c => c.Colors)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.Colors.Length).ThenBy(c => c.Colors)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.Colors);
                }
            }
            else if (ConfigurationManager.SortCriteria == "Color Identity")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.ColorIdentity.Length).ThenByDescending(c => c.ColorIdentity)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.ColorIdentity.Length).ThenBy(c => c.ColorIdentity)
                        .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.ColorIdentity);
                }
            }
            else if (ConfigurationManager.SortCriteria == "Power")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c =>
                   {
                       if (c.Power is null) return 0;
                       bool valid = Int32.TryParse(c.Power, out int power);
                       if (valid) return power;
                       else return 0;
                   }).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c =>
                    {
                        if (c.Power is null) return 0;
                        bool valid = Int32.TryParse(c.Power, out int power);
                        if (valid) return power;
                        else return 0;
                    }).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.Power ?? "");
                }
            }
            else if (ConfigurationManager.SortCriteria == "Toughness")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c =>
                    {
                        if (c.Toughness is null) return 0;
                        bool valid = Int32.TryParse(c.Toughness, out int toughness);
                        if (valid) return toughness;
                        else return 0;
                    }).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c =>
                    {
                        if (c.Toughness is null) return 0;
                        bool valid = Int32.TryParse(c.Toughness, out int toughness);
                        if (valid) return toughness;
                        else return 0;
                    }).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.Toughness ?? "");
                }
            }
            else if (ConfigurationManager.SortCriteria == "Market Price")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.MarketPrice ?? Double.PositiveInfinity)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.MarketPrice ?? Double.PositiveInfinity)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.MarketPrice?.ToString() ?? "N/A");
                }
            }
            else if (ConfigurationManager.SortCriteria == "Rarity")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.Rarity).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.Rarity).ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.Rarity);
                }
            }
            else if (ConfigurationManager.SortCriteria == "Life")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.Life ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.Life ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.Life?.ToString() ?? "N/A");
                }
            }
            else if (ConfigurationManager.SortCriteria == "Hand")
            {
                if (ConfigurationManager.DescendingSort)
                {
                    mockItems = mockItems.OrderByDescending(c => c.Hand ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                else
                {
                    mockItems = mockItems.OrderBy(c => c.Hand ?? Int32.MaxValue)
                                         .ThenBy(c => c.Cmc).ThenBy(c => c.Name);
                }
                if (ConfigurationManager.CountByGroup)
                {
                    grouping = mockItems.GroupBy(c => c.Hand?.ToString() ?? "N/A");
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            if (!ConfigurationManager.CountByGroup)
            {
                grouping = mockItems.GroupBy(c => "Total");
            }

            Items = grouping.Select(g => new EnhancedGrouping<Card>(g, labelFunc)).ToList();
        }
        
        public class CardsQuery
        {
            Expression builtExpression = null;
            static ParameterExpression param = Expression.Parameter(typeof(Card), "card");
            string Connector;

            public IEnumerable<Card> Domain { get; private set; }

            public CardsQuery Where(string field, bool set)
            {
                field = field.Replace(" ", "");
                Expression property = Expression.Property(param, field);
                Expression constant = Expression.Constant(set, typeof(bool));

                Expression fullCombine = Expression.Equal(property, constant);

                if (this.builtExpression is null)
                {
                    this.builtExpression = fullCombine;
                }
                else if (Connector == "All")
                {
                    this.builtExpression = Expression.AndAlso(builtExpression, fullCombine);
                }
                else if (Connector == "Any")
                {
                    this.builtExpression = Expression.OrElse(builtExpression, fullCombine);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Where(string field, string op, string value)
            {
                if(value is null || string.IsNullOrWhiteSpace(value))
                {
                    return this;
                }
                field = field.Replace(" ", "");
                value = value.Trim();
                if (field.Contains("Color"))
                {
                    if (value.Equals("White", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "W";
                    }
                    else if (value.Equals("Blue", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "U";
                    }
                    else if (value.Equals("Black", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "B";
                    }
                    else if (value.Equals("Red", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "R";
                    }
                    else if (value.Equals("Green", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "G";
                    }
                }

                Expression property = Expression.Property(param, field);
                Expression constant = Expression.Constant(value);
                PropertyInfo propertyInfo = typeof(Card).GetProperty(field);
                if(propertyInfo.PropertyType == typeof(bool) || op == "Exists")
                {
                    bool val = bool.Parse(value);
                    constant = Expression.Constant(val, typeof(bool));
                }
                else if(propertyInfo.PropertyType == typeof(int?))
                {
                    constant = Expression.Constant(Int32.Parse(value), typeof(int?));
                }
                else if (propertyInfo.PropertyType == typeof(int))
                {
                    constant = Expression.Constant(Int32.Parse(value), typeof(int));
                }
                else if(propertyInfo.PropertyType == typeof(double?))
                {
                    constant = Expression.Constant(Double.Parse(value), typeof(double?));
                }
                else if (propertyInfo.PropertyType == typeof(double))
                {
                    constant = Expression.Constant(Double.Parse(value), typeof(double));
                }
                Expression fullCombine = null;
                if(op == "Equals")
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        fullCombine = Expression.Call(property, "Equals", Type.EmptyTypes, new[] { constant, caseInsensitive });
                    }
                    else
                    {
                        fullCombine = Expression.Equal(property, constant);
                    }
                }
                else if(op == "Contains")
                {
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                        fullCombine = Expression.Call(typeof(StringExtensions), "Contains", Type.EmptyTypes, property, constant, caseInsensitive);
                    }
                }
                else if(op == "Like")
                {
                    fullCombine = Expression.Call(typeof(StringExtensions), "Like", Type.EmptyTypes, property, constant);
                }
                else if(op == "Starts With")
                {
                    Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                    fullCombine = Expression.Call(property, "StartsWith", Type.EmptyTypes, constant, caseInsensitive);
                }
                else if (op == "Ends With")
                {
                    Expression caseInsensitive = Expression.Constant(StringComparison.OrdinalIgnoreCase);
                    fullCombine = Expression.Call(property, "EndsWith", Type.EmptyTypes, constant, caseInsensitive);
                }
                else if(op == "Less Than")
                {
                    fullCombine = Expression.LessThan(property, constant);
                }
                else if(op == "Greater Than")
                {
                    fullCombine = Expression.GreaterThan(property, constant);
                }
                else if(op == "Exists")
                {
                    Expression nullConstant = Expression.Constant(null);
                    bool exists = Boolean.Parse(value);
                    if (exists)
                    {
                        fullCombine = Expression.NotEqual(property, nullConstant);
                    }
                    else
                    {
                        fullCombine = Expression.Equal(property, nullConstant);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (this.builtExpression is null)
                {
                    this.builtExpression = fullCombine;
                }
                else if(Connector == "All")
                {
                    this.builtExpression = Expression.AndAlso(builtExpression, fullCombine);
                }
                else if (Connector == "Any")
                {
                    this.builtExpression = Expression.OrElse(builtExpression, fullCombine);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Where(CardsQuery other)
            {
                if(other is null || other.builtExpression is null)
                {
                    throw new ArgumentNullException();
                }
                Expression otherExpression = other.builtExpression;
                if(this.builtExpression is null)
                {
                    this.builtExpression = other.builtExpression;
                }
                else if (Connector == "All")
                {
                    this.builtExpression = Expression.AndAlso(builtExpression, otherExpression);
                }
                else if (Connector == "Any")
                {
                    this.builtExpression = Expression.OrElse(builtExpression, otherExpression);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return this;
            }

            public CardsQuery Negate()
            {
                if(this.builtExpression is null)
                {
                    throw new Exception("Cannot negate an empty query");
                }
                else
                {
                    this.builtExpression = Expression.Not(this.builtExpression);
                }

                return this;
            }

            private Expression<Func<Card, bool>> BuildExpression()
            {
                if(this.builtExpression is null)
                {
                    return Expression.Lambda<Func<Card, bool>>(Expression.Constant(true), param);
                }

                return Expression.Lambda<Func<Card, bool>>(this.builtExpression, param);
            }

            public CardDataStore ToDataStore()
            {
                return new CardDataStore(this.BuildExpression(), this.Domain);
            }

            public List<Card> ToList()
            {
                if(this.Domain is null)
                {
                    return realm.All<Card>().Where(this.BuildExpression()).ToList();
                }
                else
                {
                    return this.Domain.Where(this.BuildExpression().Compile()).ToList();
                }
            }

            public CardsQuery(string connector, IEnumerable<Card> domain = null)
            {
                this.Connector = connector;
                this.Domain = domain;
            }
        }

        public static CardsQuery Where(string field, string op, string value, string connector="All")
        {
            return new CardsQuery(connector).Where(field, op, value);
        }
        public static CardsQuery Where(string field, bool set, string connector = "All")
        {
            return new CardsQuery(connector).Where(field, set);
        }

        public static Card ById(string id)
        {
            return realm.Find<Card>(id);
        }

        public static Card ByMvid(string mvid)
        {
            return realm.All<Card>().Where(c => c.MultiverseId == mvid).FirstOrDefault();
        }
    }
}