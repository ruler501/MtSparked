using MtSparked.UI.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views.Search {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DomainCriteriaCell : ContentView {

        private DomainCriteria DomainCriteria { get; set; }

        public DomainCriteriaCell(DomainCriteria criteria) {
            this.InitializeComponent();

            this.BindingContext = DomainCriteria = criteria;
        }

    }
}