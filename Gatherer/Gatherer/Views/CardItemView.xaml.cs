﻿using Gatherer.Models;
using Gatherer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Gatherer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CardItemView : ContentView
	{
        Card Card;

		public CardItemView (Card card)
		{
			InitializeComponent ();

            this.BindingContext = Card = card;
		}
        void RemoveFoi(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.RemoveCard(Card, false, 1);
        }
        void AddFoil(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.RemoveCard(Card, false, 1);
        }

        void RemoveNormal(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.RemoveCard(Card, true, 1);
        }

        void AddNormal(object sender, EventArgs e)
        {
            ConfigurationManager.ActiveDeck.RemoveCard(Card, true, 1);
        }
    }
}