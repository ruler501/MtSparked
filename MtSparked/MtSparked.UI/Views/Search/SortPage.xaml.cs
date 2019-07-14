using MtSparked.Core.Services;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views.Search {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SortPage : ContentPage {

		public SortPage () {
			this.InitializeComponent ();
            int index = this.CriteriaPicker.Items.IndexOf(ConfigurationManager.SortCriteria);
            if(index < 0) {
                index = 0;
            }
            this.CriteriaPicker.SelectedIndex = index;
            this.CountByGroupSwitch.IsToggled = ConfigurationManager.CountByGroup;
            this.DescendingSortSwitch.IsToggled = ConfigurationManager.DescendingSort;
		}

        public void OnConfirm(object sender, EventArgs args) {
            ConfigurationManager.SortCriteria = (string)this.CriteriaPicker.SelectedItem;
            ConfigurationManager.CountByGroup = this.CountByGroupSwitch.IsToggled;
            ConfigurationManager.DescendingSort = this.DescendingSortSwitch.IsToggled;

            this.Navigation.RemovePage(this);
        }

	}
}