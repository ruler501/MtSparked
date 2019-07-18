using MtSparked.Interop.Services;
using MtSparked.Interop.Models;
using MtSparked.UI.Models;
using System;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MtSparked.Interop.Databases;

namespace MtSparked.UI.Views.Search {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchCriteriaCell : ContentView, IHasCardQuery {

        private SearchCriteria SearchCriteria { get; }

        public SearchCriteriaCell(SearchCriteria criteria) {
            this.InitializeComponent();

            this.BindingContext = this.SearchCriteria = criteria;
        }

        public IQueryable<Card> GetQuery() {
            string field = this.SearchCriteria.Field.Replace(" ", "");
            PropertyInfo property = typeof(Card).GetProperty(field);
            if (property.PropertyType == typeof(bool)) {
                // TODO: Update to use BinaryOperations correctly
                // return DataStore<Card>.Where(this.SearchCriteria.Field, this.SearchCriteria.Set);
            } else if (this.SearchCriteria.Operation == "Exists") {
                // return DataStore<Card>.Where(this.SearchCriteria.Field, this.SearchCriteria.Operation, this.SearchCriteria.Set.ToString());
            } else {
                // return DataStore<Card>.Where(this.SearchCriteria.Field, this.SearchCriteria.Operation, this.SearchCriteria.Value);
            }
            return null;
        }

        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();

            this.OperationPicker.SelectedIndex = 0;
        }

        public void FieldIndexChanged(object sender, EventArgs args) {
            string field = this.SearchCriteria.Field.Replace(" ", "");
            PropertyInfo property = typeof(Card).GetProperty(field);

            if(property.PropertyType == typeof(string)) {
                string selection = this.SearchCriteria.Operation;

                if (field.Contains("Color")) {
                    int index = SearchCriteria.ColorOperations.IndexOf(selection);
                    index = index >= 0 ? index : 0;
                    this.SearchCriteria.Operations = SearchCriteria.ColorOperations;
                    if (selection == "Contains") {
                        this.ColorPicker.IsVisible = true;
                        this.ValueEntry.IsVisible = false;
                        this.SetSwitch.IsVisible = false;
                        this.OperationPicker.IsVisible = true;
                    } else {
                        this.ColorPicker.IsVisible = false;
                        this.ValueEntry.IsVisible = true;
                        this.SetSwitch.IsVisible = false;
                        this.ValueEntry.Keyboard = Keyboard.Plain;
                    }
                    this.OperationPicker.SelectedItem = SearchCriteria.ColorOperations[index];
                } else {
                    int index = SearchCriteria.StringOperations.IndexOf(selection);
                    index = index >= 0 ? index : 0;
                    this.SearchCriteria.Operations = SearchCriteria.StringOperations;

                    if (selection == "Exists") {
                        this.ValueEntry.IsVisible = false;
                        this.ColorPicker.IsVisible = false;
                        this.SetSwitch.IsVisible = true;
                        this.OperationPicker.IsVisible = true;
                    } else {
                        this.ValueEntry.IsVisible = true;
                        this.ValueEntry.Keyboard = Keyboard.Plain;
                        this.ColorPicker.IsVisible = false;
                        this.SetSwitch.IsVisible = false;
                        this.OperationPicker.IsVisible = true;
                    }
                    this.OperationPicker.SelectedItem = SearchCriteria.StringOperations[index];
                }
            } else if(property.PropertyType == typeof(int) || property.PropertyType == typeof(double)) {
                string selection = this.SearchCriteria.Operation;
                int index = SearchCriteria.NumberOperations.IndexOf(selection);
                if(index  <= 0) {
                    index = 0;
                }
                this.SearchCriteria.Operations = SearchCriteria.NumberOperations;
                this.ValueEntry.IsVisible = true;
                this.ValueEntry.Keyboard = Keyboard.Numeric;
                this.ColorPicker.IsVisible = false;
                this.SetSwitch.IsVisible = false;
                this.OperationPicker.IsVisible = true;
                this.OperationPicker.SelectedItem = SearchCriteria.NumberOperations[index];
            } else if (property.PropertyType == typeof(int?) || property.PropertyType == typeof(double?)) {
                string selection = this.SearchCriteria.Operation;
                int index = SearchCriteria.NullableNumberOperations.IndexOf(selection);
                index = index >= 0 ? index : 0;
                this.SearchCriteria.Operations = SearchCriteria.NullableNumberOperations;
                this.ValueEntry.IsVisible = true;
                this.ValueEntry.Keyboard = Keyboard.Numeric;
                this.ColorPicker.IsVisible = false;
                this.SetSwitch.IsVisible = false;
                this.OperationPicker.IsVisible = true;
                this.OperationPicker.SelectedItem = SearchCriteria.NullableNumberOperations[index];
            } else if(property.PropertyType == typeof(bool)) {
                this.ColorPicker.IsVisible = false;
                this.ValueEntry.IsVisible = false;
                this.SetSwitch.IsVisible = true;
                this.OperationPicker.IsVisible = false;
            }
        }

        public void OperationIndexChanged(object sender, EventArgs args) {
            string operation = this.SearchCriteria.Operation;
            if(operation is null) {
                operation =  this.SearchCriteria.Operations[0];
                Device.BeginInvokeOnMainThread(() => this.OperationPicker.SelectedIndex = 0);
            }
            string field = this.SearchCriteria.Field.Replace(" ", "");
            PropertyInfo property = typeof(Card).GetProperty(field);
            if (operation == "Exists" || property.PropertyType == typeof(bool)) {
                this.ColorPicker.IsVisible = false;
                this.ValueEntry.IsVisible = false;
                this.SetSwitch.IsVisible = true;
                this.OperationPicker.IsVisible = false;
            } else if(operation == "Contains" && property.PropertyType == typeof(string) && field.Contains("Color")) {
                this.ColorPicker.IsVisible = true;
                this.ValueEntry.IsVisible = false;
                this.SetSwitch.IsVisible = false;
                this.OperationPicker.IsVisible = true;
            } else {
                this.ColorPicker.IsVisible = false;
                this.ValueEntry.IsVisible = true;
                this.SetSwitch.IsVisible = false;
                this.OperationPicker.IsVisible = true;
            }
        }

    }
}