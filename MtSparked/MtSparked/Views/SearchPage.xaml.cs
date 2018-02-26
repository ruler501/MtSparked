using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MtSparked.Models;
using MtSparked.Views;
using MtSparked.ViewModels;
using MtSparked.Services;

namespace MtSparked.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage
	{

        public SearchPage()
        {
            InitializeComponent();

            this.RootGroup.AddItem(null, null);
        }

        async void Search(object sender, EventArgs e)
        {
            CardDataStore.CardsQuery query = RootGroup.GetQuery();
            await Navigation.PushAsync(new CardsListPage(query.ToDataStore()));
        }

        void Clear(object sender, EventArgs e)
        {
            this.RootGroup.Clear();
            this.RootGroup.AddItem(null, null);
        }
    }
}