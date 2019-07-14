using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MtSparked.Interop.Models;
using MtSparked.Core.Services;

namespace MtSparked.UI.Views.Search {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage {

        public SearchPage() {
            this.InitializeComponent();

            this.RootGroup.AddItem(null, null);
        }

        private async void Search(object sender, EventArgs e) {
            CardDataStore.CardsQuery query = this.RootGroup.GetQuery();
            await this.Navigation.PushAsync(new CardsListPage(query.ToDataStore()));
        }

        private void Clear(object sender, EventArgs e) {
            this.RootGroup.Clear();
            this.RootGroup.AddItem(null, null);
        }

        private async void SetDomain(object sender, EventArgs e) => await this.Navigation.PushAsync(new DomainPage(this.OnDomainConfirmed));

        private void OnDomainConfirmed(IEnumerable<Card> domain) => this.RootGroup.SetDomain(domain);
        
    }
}