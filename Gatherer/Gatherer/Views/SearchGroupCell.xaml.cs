﻿using Gatherer.Models;
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
	public partial class SearchGroupCell : ContentView, IQueryable
    {
        SearchViewModel viewModel;

        public SearchGroupCell ()
            : this(new SearchViewModel())
		{
        }

        public SearchGroupCell(SearchViewModel model)
        {
            InitializeComponent();

            this.BindingContext = viewModel = model;

            this.CreateChildren();
        }

        async void AddItem(object sender, EventArgs e)
        {
            SearchCriteria criteria = viewModel.AddCriteria();
            this.AddCriteria(criteria);
        }

        async void AddGroup(object sender, EventArgs e)
        {
            SearchViewModel model = viewModel.AddGroup();
            this.AddModel(model);
        }
        
        protected void CreateChildren() { 
            this.StackLayout.Children.Clear();
            foreach(object value in viewModel.Items)
            {
                if(value is SearchViewModel model)
                {
                    this.AddModel(model);
                }
                else if(value is SearchCriteria criteria){
                    this.AddCriteria(criteria);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected void AddModel(SearchViewModel model)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(19, GridUnitType.Star) });

            grid.Children.Add(new Frame(), 0, 0);
            grid.Children.Add(new SearchGroupCell(model), 1, 0);
            this.StackLayout.Children.Add(grid);
        }

        protected void AddCriteria(SearchCriteria criteria)
        {
            this.StackLayout.Children.Add(new SearchCriteriaCell(criteria));
        }

        public CardDataStore.CardsQuery GetQuery()
        {
            if ((BindingContext as SearchViewModel) is null)
            {
                throw new Exception("Invalid Binding Context");
            }

            SearchViewModel model = (SearchViewModel)BindingContext;

            return model.CreateQuery();
        }
    }
}