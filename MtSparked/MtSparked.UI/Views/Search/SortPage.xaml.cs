using MtSparked.Interop.Services;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views.Search {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SortPage : ContentPage {

		public SortPage () {
			this.InitializeComponent ();
            // TODO: Update to use current queries SortCriteria
            // int index = this.CriteriaPicker.Items.IndexOf(ConfigurationManager.SortCriteria);
            int index = 0;
            if (index < 0) {
                index = 0;
            }
            this.CriteriaPicker.SelectedIndex = index;
            this.CountByGroupSwitch.IsToggled = ConfigurationManager.CountByGroup;
            // TODO: Use Reversed from the SortCriteria
            // this.DescendingSortSwitch.IsToggled = ConfigurationManager.DescendingSort;
		}

        public void OnConfirm(object sender, EventArgs args) {
            // ConfigurationManager.SortCriteria = (string)this.CriteriaPicker.SelectedItem;
            ConfigurationManager.CountByGroup = this.CountByGroupSwitch.IsToggled;
            // TODO: Update to use current queries Reversed
            // ConfigurationManager.DescendingSort = this.DescendingSortSwitch.IsToggled;

            this.Navigation.RemovePage(this);
        }

	}
}