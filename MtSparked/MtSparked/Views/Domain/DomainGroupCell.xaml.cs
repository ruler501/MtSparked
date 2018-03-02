using MtSparked.Models;
using MtSparked.Services;
using MtSparked.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DomainGroupCell : ContentView
    {
        DomainViewModel viewModel;

        public DomainGroupCell()
            : this(new DomainViewModel())
        {
        }

        public DomainGroupCell(DomainViewModel model)
        {
            InitializeComponent();

            this.BindingContext = viewModel = model;

            this.CreateChildren();
        }

        public void AddItem(object sender, EventArgs e)
        {
            DomainCriteria criteria = viewModel.AddCriteria();
            this.AddCriteria(criteria);
        }

        void AddGroup(object sender, EventArgs e)
        {
            DomainViewModel model = viewModel.AddGroup();
            this.AddModel(model);
        }

        protected void CreateChildren()
        {
            this.StackLayout.Children.Clear();
            foreach (object value in viewModel.Items)
            {
                if (value is DomainViewModel model)
                {
                    this.AddModel(model);
                }
                else if (value is DomainCriteria criteria)
                {
                    this.AddCriteria(criteria);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        protected void AddModel(DomainViewModel model)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(19, GridUnitType.Star) });

            grid.Children.Add(new Frame()
            {
                IsVisible = false
            }, 0, 0);
            grid.Children.Add(new DomainGroupCell(model), 1, 0);
            this.StackLayout.Children.Add(grid);
        }

        protected void AddCriteria(DomainCriteria criteria)
        {
            this.StackLayout.Children.Add(new DomainCriteriaCell(criteria));
        }

        public IEnumerable<Card> CreateDomain()
        {
            if ((BindingContext as DomainViewModel) is null)
            {
                throw new Exception("Invalid Binding Context");
            }

            DomainViewModel model = (DomainViewModel)BindingContext;

            return model.CreateDomain();
        }

        public void Clear()
        {
            this.viewModel.Items.Clear();
            this.StackLayout.Children.Clear();
        }
    }
}