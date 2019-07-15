using MtSparked.Interop.Models;
using MtSparked.Interop.Services;
using MtSparked.UI.ViewModels;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MtSparked.UI.Models;
using MtSparked.Interop.Databases;

namespace MtSparked.UI.Views.Search {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchGroupCell : ContentView, IHasCardQuery {
        private SearchViewModel ViewModel { get; }
        private IEnumerable<Card> Domain { get; set; }

        public SearchGroupCell ()
            : this(new SearchViewModel())
		{}

        public SearchGroupCell(SearchViewModel model) {
            this.InitializeComponent();

            this.BindingContext = this.ViewModel = model;

            this.CreateChildren();
        }

        public void AddItem(object sender, EventArgs e) {
            SearchCriteria criteria = this.ViewModel.AddCriteria();
            this.AddCriteria(criteria);
        }

        public void AddGroup(object sender, EventArgs e) {
            SearchViewModel model = this.ViewModel.AddGroup();
            this.AddModel(model);
        }

        protected void CreateChildren() { 
            this.StackLayout.Children.Clear();
            foreach(object value in this.ViewModel.Items) {
                if(value is SearchViewModel model) {
                    this.AddModel(model);
                } else if(value is SearchCriteria criteria) {
                    this.AddCriteria(criteria);
                } else {
                    throw new NotImplementedException();
                }
            }
        }

        protected void AddModel(SearchViewModel model) {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(19, GridUnitType.Star) });

            grid.Children.Add(new Frame() { IsVisible = false }, 0, 0);
            grid.Children.Add(new SearchGroupCell(model), 1, 0);
            this.StackLayout.Children.Add(grid);
        }

        protected void AddCriteria(SearchCriteria criteria) => this.StackLayout.Children.Add(new SearchCriteriaCell(criteria));

        public DataStore<Card>.IQuery GetQuery() {
            SearchViewModel model = this.ViewModel;

            return model.CreateQuery(this.Domain);
        }

        public void Clear() {
            this.ViewModel.Items.Clear();
            this.StackLayout.Children.Clear();
        }

        private double translatedX = 0;
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e) {
            switch (e.StatusType) { 
            case GestureStatus.Running:
                this.ControlGrid.TranslationX = this.translatedX + e.TotalX ;
                break;
            case GestureStatus.Completed:
                // Store the translation applied during the pan
                this.translatedX = this.Content.TranslationX;
                break;
            default:
                throw new NotImplementedException();
            }
        }


        // TODO #90: Remove Duplicate Setter Method for SearchGroupCell.Domain
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
        public void SetDomain(IEnumerable<Card> domain) {
            this.Domain = domain;
        }
    }
}