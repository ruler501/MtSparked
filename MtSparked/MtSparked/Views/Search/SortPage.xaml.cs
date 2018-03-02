using MtSparked.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SortPage : ContentPage
	{
		public SortPage ()
		{
			InitializeComponent ();

            int index = this.CriteriaPicker.Items.IndexOf(ConfigurationManager.SortCriteria);
            index = index >= 0 ? index : 0;

            this.CriteriaPicker.SelectedIndex = index;

            this.CountByGroupSwitch.IsToggled = ConfigurationManager.CountByGroup;

            this.DescendingSortSwitch.IsToggled = ConfigurationManager.DescendingSort;
		}

        public void OnConfirm(object sender, EventArgs args)
        {
            ConfigurationManager.SortCriteria = (string)this.CriteriaPicker.SelectedItem;
            ConfigurationManager.CountByGroup = this.CountByGroupSwitch.IsToggled;
            ConfigurationManager.DescendingSort = this.DescendingSortSwitch.IsToggled;

            Navigation.RemovePage(this);
        }
	}
}