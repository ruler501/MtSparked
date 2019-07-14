using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using MtSparked.Interop.Models;

namespace MtSparked.UI.Views.Search {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DomainPage : ContentPage {

        private Action<IEnumerable<Card>> OnConfirm { get; }

        public DomainPage(Action<IEnumerable<Card>> onComplete) {
            this.InitializeComponent();

            this.OnConfirm = onComplete;

            this.RootGroup.AddItem(null, null);
        }

        private void Confirm(object sender, EventArgs e) {
            IEnumerable<Card> domain = this.RootGroup.CreateDomain();

            this.OnConfirm(domain);

            this.Navigation.RemovePage(this);
        }

        private void Clear(object sender, EventArgs e) {
            this.RootGroup.Clear();
            this.RootGroup.AddItem(null, null);
        }

    }
}