using System;
using System.Collections.Generic;
using MtSparked.Interop.Models;
using MtSparked.UI.Models;
using MtSparked.UI.ViewModels.Search;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views.Search {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DomainGroupCell : ContentView {

        private DomainViewModel ViewModel { get; }

        public DomainGroupCell()
            : this(new DomainViewModel())
        {}

        public DomainGroupCell(DomainViewModel model) {
            this.InitializeComponent();

            this.BindingContext = this.ViewModel = model;

            this.CreateChildren();
        }

        public void AddItem(object sender, EventArgs e) {
            DomainCriteria criteria = this.ViewModel.AddCriteria();
            this.AddCriteria(criteria);
        }

        private void AddGroup(object sender, EventArgs e) {
            DomainViewModel model = this.ViewModel.AddGroup();
            this.AddModel(model);
        }

        protected void CreateChildren() {
            this.StackLayout.Children.Clear();
            foreach (object value in this.ViewModel.Items) {
                if (value is DomainViewModel model) {
                    this.AddModel(model);
                } else if (value is DomainCriteria criteria) {
                    this.AddCriteria(criteria);
                } else {
                    throw new NotImplementedException();
                }
            }
        }

        protected void AddModel(DomainViewModel model) {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(19, GridUnitType.Star) });

            grid.Children.Add(new Frame() { IsVisible = false }, 0, 0);
            grid.Children.Add(new DomainGroupCell(model), 1, 0);
            this.StackLayout.Children.Add(grid);
        }

        protected void AddCriteria(DomainCriteria criteria) => this.StackLayout.Children.Add(new DomainCriteriaCell(criteria));

        public IEnumerable<Card> CreateDomain() => this.ViewModel.CreateDomain();

        public void Clear() {
            this.ViewModel.Items.Clear();
            this.StackLayout.Children.Clear();
        }

    }
}