﻿using MtSparked.Interop.Models;
using Syncfusion.SfDataGrid.XForms;
using System.Collections;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MtSparked.UI.Views.Decks {
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BoardEditing : ContentPage {

        private Deck Deck { get; }

        public BoardEditing(Deck deck) {
            this.BindingContext = this.Deck = deck;

            this.InitializeComponent();
        }

        private void OnRowDragging(object sender, QueryRowDraggingEventArgs e) {
            //e.To returns the index of the current row.
            //e.From returns the index of the dragged row.
            if (e.Reason == QueryRowDraggingReason.DragEnded && e.From != e.To) {
                IList collection = (sender as SfDataGrid).ItemsSource as IList;
                int count = collection.Count;
                collection.RemoveAt(e.From - 1);
                collection.Insert(e.To - 1 - (e.From < e.To && e.To != count ? 1 : 0), e.RowData);
                e.Cancel = true;
            }
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            this.Deck.BoardInfoRefreshed();
        }

    }
}