using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Gatherer.Models;
using Gatherer.Views;
using Gatherer.ViewModels;
using Gatherer.Services;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage
	{

        public SearchPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void Search(object sender, EventArgs e)
        {
            CardDataStore.CardsQuery query = RootGroup.GetQuery();
            await Navigation.PushAsync(new CardPage(query.Find()));
        }
    }
}