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
    public partial class DomainPage : ContentPage
    {
        Action<IEnumerable<Card>> OnConfirm { get; set; }

        public DomainPage(Action<IEnumerable<Card>> onComplete)
        {
            InitializeComponent();

            this.OnConfirm = onComplete;

            this.RootGroup.AddItem(null, null);
        }

        void Confirm(object sender, EventArgs e)
        {
            IEnumerable<Card> domain = this.RootGroup.CreateDomain();

            this.OnConfirm(domain);

            Navigation.RemovePage(this);
        }

        void Clear(object sender, EventArgs e)
        {
            this.RootGroup.Clear();
            this.RootGroup.AddItem(null, null);
        }
    }
}