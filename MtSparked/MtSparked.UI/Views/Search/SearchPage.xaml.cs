using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MtSparked.Interop.Models;
using System.Linq;

namespace MtSparked.UI.Views.Search {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SearchPage : ContentPage {

        public SearchPage() {
            this.InitializeComponent();

            this.RootGroup.AddItem(null, null);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async void Search(object sender, EventArgs e) {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                              // IQueryable<Card> query = this.RootGroup.GetQuery();
                              // TODO: Get the extensions working correctly with IQuery
                              // await this.Navigation.PushAsync(new CardsListPage(query.ToDataStore()));
        }

        private void Clear(object sender, EventArgs e) {
            this.RootGroup.Clear();
            this.RootGroup.AddItem(null, null);
        }

        private async void SetDomain(object sender, EventArgs e) => await this.Navigation.PushAsync(new DomainPage(this.OnDomainConfirmed));

        private void OnDomainConfirmed(IEnumerable<Card> domain) => this.RootGroup.SetDomain(domain);
        
    }
}