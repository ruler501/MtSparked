using Gatherer.Models;
using Syncfusion.SfDataGrid.XForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BoardEditing : ContentPage
	{
        Deck Deck;

		public BoardEditing (Deck deck)
        {
            this.BindingContext = this.Deck = deck;

            InitializeComponent ();
        }

        private void OnRowDragging(object sender, QueryRowDraggingEventArgs e)
        {
            //e.To returns the index of the current row.
            //e.From returns the index of the dragged row.
            if (e.Reason == QueryRowDraggingReason.DragEnded && e.From != e.To)
            {
                var collection = (sender as SfDataGrid).ItemsSource as IList;
                int count = collection.Count;
                collection.RemoveAt(e.From - 1);
                collection.Insert(e.To - 1 - (e.From < e.To && e.To != count ? 1 : 0), e.RowData);
                e.Cancel = true;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.Deck.BoardInfoRefreshed();
        }
    }
}